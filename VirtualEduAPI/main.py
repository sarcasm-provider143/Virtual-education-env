from fastapi import FastAPI, Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer, OAuth2PasswordRequestForm
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker, Session
from pydantic import BaseModel
import datetime
import jwt
import hashlib

from models import KURSCourse, KURSResults, KURSEnrollments, KURSStudent, KURSTeacher, KURSUser

SECRET_KEY = "super_secret_key_for_virtual_edu"
ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_MINUTES = 120

SQLALCHEMY_DATABASE_URL = "mssql+pyodbc://user21_db:user21@stud-mssql.sttec.yar.ru,38325/user21_db?driver=ODBC+Driver+17+for+SQL+Server"
engine = create_engine(SQLALCHEMY_DATABASE_URL)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

app = FastAPI(
    title="ВОС API с авторизацией",
    description="API для работы с курсами и оценками",
    version="1.0.0",
    docs_url="/docs"
)

oauth2_scheme = OAuth2PasswordBearer(tokenUrl="login")


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()


def create_access_token(data: dict):
    to_encode = data.copy()
    expire = datetime.datetime.utcnow() + datetime.timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    to_encode.update({"exp": expire})
    return jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)


def get_current_user(token: str = Depends(oauth2_scheme), db: Session = Depends(get_db)):
    credentials_exception = HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Недействительный токен или сессия истекла",
        headers={"WWW-Authenticate": "Bearer"},
    )
    try:
        payload = jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
        user_id = payload.get("sub")
        if user_id is None:
            raise credentials_exception
    except jwt.PyJWTError:
        raise credentials_exception

    user = db.query(KURSUser).filter(KURSUser.user_id == int(user_id)).first()
    if user is None:
        raise credentials_exception
    return user


def require_role(allowed_roles: list[int]):
    def role_checker(current_user: KURSUser = Depends(get_current_user)):
        if current_user.role_id not in allowed_roles:
            raise HTTPException(status_code=403, detail="Доступ запрещён. Недостаточно прав.")
        return current_user

    return role_checker


class EnrollmentCreate(BaseModel):
    student_id: int
    course_id: int

class EnrollmentUpdate(BaseModel):
    status: str


@app.post("/login", tags=["Авторизация"])
def login(form_data: OAuth2PasswordRequestForm = Depends(), db: Session = Depends(get_db)):
    user = db.query(KURSUser).filter(KURSUser.login == form_data.username).first()
    if not user:
        raise HTTPException(status_code=401, detail="Неверный логин или пароль")

    entered_hash = hashlib.sha256(form_data.password.encode('utf-8')).hexdigest()
    if user.password_hash != entered_hash:
        raise HTTPException(status_code=401, detail="Неверный логин или пароль")

    access_token = create_access_token(data={"sub": str(user.user_id), "role": user.role_id})
    return {"access_token": access_token, "token_type": "bearer", "role_id": user.role_id}


@app.get("/courses", tags=["Курсы"])
def get_courses(current_user: KURSUser = Depends(get_current_user), db: Session = Depends(get_db)):
    if current_user.role_id == 1:
        courses = db.query(KURSCourse).all()
    elif current_user.role_id == 2:
        teacher = db.query(KURSTeacher).filter(KURSTeacher.user_id == current_user.user_id).first()
        courses = db.query(KURSCourse).filter(KURSCourse.teacher_id == teacher.id).all()
    else:
        student = db.query(KURSStudent).filter(KURSStudent.user_id == current_user.user_id).first()
        enrollments = db.query(KURSEnrollments).filter(
            KURSEnrollments.student_id == student.id,
            KURSEnrollments.status == "активен"
        ).all()
        courses = [e.course for e in enrollments]

    return [
        {
            "id": c.id,
            "title": c.title,
            "hours": c.hours,
            "start_date": c.start_date,
            "teacher_name": f"{c.teacher.user.last_name} {c.teacher.user.name}"
        } for c in courses
    ]


@app.post("/enrollments", status_code=201, tags=["Записи на курсы"])
def create_enrollment(
        enrollment: EnrollmentCreate,
        current_user: KURSUser = Depends(require_role([1, 2])),
        db: Session = Depends(get_db)
):
    student = db.query(KURSStudent).filter(KURSStudent.id == enrollment.student_id).first()
    course = db.query(KURSCourse).filter(KURSCourse.id == enrollment.course_id).first()

    if not student or not course:
        raise HTTPException(status_code=404, detail="Студент или курс не найден")

    if student.group.specialty_id != course.specialty_id:
        raise HTTPException(status_code=400,
                            detail="Специальность группы студента не соответствует специальности курса")

    existing = db.query(KURSEnrollments).filter(
        KURSEnrollments.student_id == enrollment.student_id,
        KURSEnrollments.course_id == enrollment.course_id
    ).first()

    if existing:
        if existing.status.lower() == "активен":
            raise HTTPException(status_code=400, detail="Студент уже активно учится на этом курсе")
        existing.status = "активен"
        existing.enrolled_at = datetime.datetime.now()
        db.commit()
        return {"message": "Студент восстановлен на курсе"}

    new_enrollment = KURSEnrollments(
        student_id=enrollment.student_id,
        course_id=enrollment.course_id,
        enrolled_at=datetime.datetime.now(),
        status="активен"
    )
    db.add(new_enrollment)
    db.commit()
    return {"message": "Студент успешно записан на курс"}


@app.get("/results/student/{student_id}", tags=["Оценки"])
def get_student_results(
        student_id: int,
        current_user: KURSUser = Depends(get_current_user),
        db: Session = Depends(get_db)
):
    # Проверяем, существует ли запрашиваемый студент
    student = db.query(KURSStudent).filter(KURSStudent.id == student_id).first()
    if not student:
        raise HTTPException(status_code=404, detail="Студент не найден")

    # 1. Если это студент, он может смотреть только свои оценки
    if current_user.role_id == 3:
        my_profile = db.query(KURSStudent).filter(KURSStudent.user_id == current_user.user_id).first()
        if not my_profile or my_profile.id != student_id:
            raise HTTPException(status_code=403, detail="Студент может просматривать только свои оценки")

        # Студент видит оценки по всем своим активным/завершенным курсам
        results = db.query(KURSResults).join(KURSEnrollments).filter(
            KURSEnrollments.student_id == student_id
        ).all()

    # 2. Если это преподаватель
    elif current_user.role_id == 2:
        teacher = db.query(KURSTeacher).filter(KURSTeacher.user_id == current_user.user_id).first()

        # Проверяем, записан ли этот студент на курсы этого преподавателя (со статусом активен или завершен)
        is_my_student = db.query(KURSEnrollments).join(KURSCourse).filter(
            KURSEnrollments.student_id == student_id,
            KURSCourse.teacher_id == teacher.id,
            KURSEnrollments.status.in_(["активен", "завершён"])
        ).first()

        if not is_my_student:
            raise HTTPException(status_code=403,
                                detail="Вы можете просматривать оценки только студентов, обучающихся на ваших курсах")

        # Преподаватель видит оценки студента только по своим курсам
        results = db.query(KURSResults).join(KURSEnrollments).join(KURSCourse).filter(
            KURSEnrollments.student_id == student_id,
            KURSCourse.teacher_id == teacher.id
        ).all()

    # 3. Если это админ, он может смотреть всё
    else:
        results = db.query(KURSResults).join(KURSEnrollments).filter(
            KURSEnrollments.student_id == student_id
        ).all()

    if not results:
        return {"message": "У студента пока нет оценок по доступным вам курсам"}

    return [
        {
            "course_title": r.enrollment.course.title,
            "grade": r.grade,
            "date": r.date,
            "comment": r.comment
        } for r in results
    ]


# Изменение записи на курс (админ)
@app.put("/enrollments/{enrollment_id}", tags=["Админ: Управление записями"])
def update_enrollment_status(
    enrollment_id: int,
    data: EnrollmentUpdate,
    current_user: KURSUser = Depends(require_role([1])), # Строго роль 1 (Админ)
    db: Session = Depends(get_db)
):
    # Проверяем, что передан корректный статус
    if data.status not in ["активен", "завершён", "отчислен"]:
        raise HTTPException(
            status_code=400,
            detail="Недопустимый статус. Разрешены: 'активен', 'завершён', 'отчислен'"
        )

    enrollment = db.query(KURSEnrollments).filter(KURSEnrollments.id == enrollment_id).first()
    if not enrollment:
        raise HTTPException(status_code=404, detail="Запись на курс не найдена")

    # Обновляем статус
    enrollment.status = data.status
    db.commit()

    return {
        "message": "Статус записи успешно обновлен",
        "enrollment_id": enrollment.id,
        "new_status": enrollment.status
    }

# Удаление пользователей (админ)
@app.delete("/users/{user_id}", tags=["Админ: Управление пользователями"])
def delete_user(
        user_id: int,
        current_user: KURSUser = Depends(require_role([1])),  # Строго роль 1 (Админ)
        db: Session = Depends(get_db)
):
    # Нельзя удалить самого себя
    if current_user.user_id == user_id:
        raise HTTPException(status_code=400, detail="Нельзя удалить собственную учетную запись")

    user = db.query(KURSUser).filter(KURSUser.user_id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail="Пользователь не найден")

    # Проверяем, кто это - студент или преподаватель
    student = db.query(KURSStudent).filter(KURSStudent.user_id == user_id).first()
    teacher = db.query(KURSTeacher).filter(KURSTeacher.user_id == user_id).first()

    if student:
        # Удаляем оценки студента
        enrollments = db.query(KURSEnrollments).filter(KURSEnrollments.student_id == student.id).all()
        for e in enrollments:
            db.query(KURSResults).filter(KURSResults.enrollment_id == e.id).delete()
            db.delete(e)
        # Удаляем профиль студента
        db.delete(student)

    elif teacher:
        # Если это преподаватель, проверяем, есть ли у него курсы
        courses = db.query(KURSCourse).filter(KURSCourse.teacher_id == teacher.id).all()
        if courses:
            raise HTTPException(status_code=400,
                                detail="Нельзя удалить преподавателя, у которого есть курсы. Сначала удалите его курсы.")
        db.delete(teacher)

    # Удаляем саму учетную запись
    db.delete(user)
    db.commit()

    return {"message": "Пользователь успешно удален"}