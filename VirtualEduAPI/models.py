from typing import Optional
import datetime

from sqlalchemy import Date, DateTime, ForeignKeyConstraint, Identity, Index, Integer, NCHAR, PrimaryKeyConstraint, String, Unicode, text
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship

class Base(DeclarativeBase):
    pass

class KURSRole(Base):
    __tablename__ = 'KURS_role'
    __table_args__ = (PrimaryKeyConstraint('id', name='PK_KURS_role'),)
    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    name: Mapped[str] = mapped_column(String(50, 'Cyrillic_General_CI_AS'), nullable=False)
    KURS_user: Mapped[list['KURSUser']] = relationship('KURSUser', back_populates='role')

class KURSSpecialities(Base):
    __tablename__ = 'KURS_specialities'
    __table_args__ = (PrimaryKeyConstraint('id', name='PK_KURS_specialities'), Index('UQ_code', 'code', mssql_clustered=False, unique=True))
    id: Mapped[int] = mapped_column(Integer, Identity(start=1, increment=1), primary_key=True, autoincrement=True)
    code: Mapped[str] = mapped_column(String(20, 'Cyrillic_General_CI_AS'), nullable=False)
    name: Mapped[str] = mapped_column(String(150, 'Cyrillic_General_CI_AS'), nullable=False)
    KURS_group: Mapped[list['KURSGroup']] = relationship('KURSGroup', back_populates='specialty')
    KURS_course: Mapped[list['KURSCourse']] = relationship('KURSCourse', back_populates='specialty')

class KURSGroup(Base):
    __tablename__ = 'KURS_group'
    __table_args__ = (ForeignKeyConstraint(['specialty_id'], ['KURS_specialities.id'], name='FK_KURS_group_KURS_specialities'), PrimaryKeyConstraint('id', name='PK_KURS_group'))
    id: Mapped[int] = mapped_column(Integer, Identity(start=1, increment=1), primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column(String(20, 'Cyrillic_General_CI_AS'), nullable=False)
    specialty_id: Mapped[int] = mapped_column(Integer, nullable=False)
    specialty: Mapped['KURSSpecialities'] = relationship('KURSSpecialities', back_populates='KURS_group')
    KURS_student: Mapped[list['KURSStudent']] = relationship('KURSStudent', back_populates='group')

class KURSUser(Base):
    __tablename__ = 'KURS_user'
    __table_args__ = (ForeignKeyConstraint(['role_id'], ['KURS_role.id'], name='FK_KURS_user_KURS_role'), PrimaryKeyConstraint('user_id', name='PK_KURS_user'))
    user_id: Mapped[int] = mapped_column(Integer, Identity(start=1, increment=1), primary_key=True, autoincrement=True)
    last_name: Mapped[str] = mapped_column(String(50, 'Cyrillic_General_CI_AS'), nullable=False)
    name: Mapped[str] = mapped_column(String(50, 'Cyrillic_General_CI_AS'), nullable=False)
    login: Mapped[str] = mapped_column(String(50, 'Cyrillic_General_CI_AS'), nullable=False)
    password_hash: Mapped[str] = mapped_column(String(256, 'Cyrillic_General_CI_AS'), nullable=False)
    email: Mapped[str] = mapped_column(String(100, 'Cyrillic_General_CI_AS'), nullable=False)
    role_id: Mapped[int] = mapped_column(Integer, nullable=False)
    middle_name: Mapped[Optional[str]] = mapped_column(String(50, 'Cyrillic_General_CI_AS'))
    role: Mapped['KURSRole'] = relationship('KURSRole', back_populates='KURS_user')
    KURS_admin: Mapped[list['KURSAdmin']] = relationship('KURSAdmin', back_populates='user')
    KURS_student: Mapped[list['KURSStudent']] = relationship('KURSStudent', back_populates='user')
    KURS_teacher: Mapped[list['KURSTeacher']] = relationship('KURSTeacher', back_populates='user')

class KURSAdmin(Base):
    __tablename__ = 'KURS_admin'
    __table_args__ = (ForeignKeyConstraint(['user_id'], ['KURS_user.user_id'], name='FK_KURS_admin_KURS_user'), PrimaryKeyConstraint('id', name='PK_KURS_admin'), Index('UQ_admin_user_id', 'user_id', mssql_clustered=False, unique=True))
    id: Mapped[int] = mapped_column(Integer, Identity(start=1, increment=1), primary_key=True, autoincrement=True)
    user_id: Mapped[int] = mapped_column(Integer, nullable=False)
    position: Mapped[str] = mapped_column(NCHAR(150, 'Cyrillic_General_CI_AS'), nullable=False)
    user: Mapped['KURSUser'] = relationship('KURSUser', back_populates='KURS_admin')

class KURSStudent(Base):
    __tablename__ = 'KURS_student'
    __table_args__ = (ForeignKeyConstraint(['group_id'], ['KURS_group.id'], name='FK_KURS_student_KURS_group'), ForeignKeyConstraint(['user_id'], ['KURS_user.user_id'], name='FK_KURS_student_KURS_user'), PrimaryKeyConstraint('id', name='PK_KURS_student'), Index('UQ_student_user_id', 'user_id', mssql_clustered=False, unique=True))
    id: Mapped[int] = mapped_column(Integer, Identity(start=1, increment=1), primary_key=True, autoincrement=True)
    user_id: Mapped[int] = mapped_column(Integer, nullable=False)
    group_id: Mapped[int] = mapped_column(Integer, nullable=False)
    phone: Mapped[str] = mapped_column(String(12, 'Cyrillic_General_CI_AS'), nullable=False)
    group: Mapped['KURSGroup'] = relationship('KURSGroup', back_populates='KURS_student')
    user: Mapped['KURSUser'] = relationship('KURSUser', back_populates='KURS_student')
    KURS_enrollments: Mapped[list['KURSEnrollments']] = relationship('KURSEnrollments', back_populates='student')

class KURSTeacher(Base):
    __tablename__ = 'KURS_teacher'
    __table_args__ = (ForeignKeyConstraint(['user_id'], ['KURS_user.user_id'], name='FK_KURS_teacher_KURS_user'), PrimaryKeyConstraint('id', name='PK_KURS_teacher'), Index('UQ_teacher_user_id', 'user_id', mssql_clustered=False, unique=True))
    id: Mapped[int] = mapped_column(Integer, Identity(start=1, increment=1), primary_key=True, autoincrement=True)
    user_id: Mapped[int] = mapped_column(Integer, nullable=False)
    position: Mapped[str] = mapped_column(String(150, 'Cyrillic_General_CI_AS'), nullable=False)
    degree: Mapped[str] = mapped_column(String(150, 'Cyrillic_General_CI_AS'), nullable=False)
    user: Mapped['KURSUser'] = relationship('KURSUser', back_populates='KURS_teacher')
    KURS_course: Mapped[list['KURSCourse']] = relationship('KURSCourse', back_populates='teacher')

class KURSCourse(Base):
    __tablename__ = 'KURS_course'
    __table_args__ = (ForeignKeyConstraint(['specialty_id'], ['KURS_specialities.id'], name='FK_KURS_course_KURS_specialities'), ForeignKeyConstraint(['teacher_id'], ['KURS_teacher.id'], name='FK_Courses_Teachers'), PrimaryKeyConstraint('id', name='PK_courses'), Index('UQ_Courses_teacher_title', 'teacher_id', 'title', mssql_clustered=False, unique=True))
    id: Mapped[int] = mapped_column(Integer, Identity(start=1, increment=1), primary_key=True, autoincrement=True)
    teacher_id: Mapped[int] = mapped_column(Integer, nullable=False)
    title: Mapped[str] = mapped_column(String(150, 'Cyrillic_General_CI_AS'), nullable=False)
    hours: Mapped[int] = mapped_column(Integer, nullable=False)
    start_date: Mapped[datetime.date] = mapped_column(Date, nullable=False)
    specialty_id: Mapped[int] = mapped_column(Integer, nullable=False)
    description: Mapped[Optional[str]] = mapped_column(String(500, 'Cyrillic_General_CI_AS'))
    specialty: Mapped['KURSSpecialities'] = relationship('KURSSpecialities', back_populates='KURS_course')
    teacher: Mapped['KURSTeacher'] = relationship('KURSTeacher', back_populates='KURS_course')
    KURS_enrollments: Mapped[list['KURSEnrollments']] = relationship('KURSEnrollments', back_populates='course')
    KURS_materials: Mapped[list['KURSMaterials']] = relationship('KURSMaterials', back_populates='course')

class KURSEnrollments(Base):
    __tablename__ = 'KURS_enrollments'
    __table_args__ = (ForeignKeyConstraint(['course_id'], ['KURS_course.id'], name='FK_KURS_enrollments_courses'), ForeignKeyConstraint(['student_id'], ['KURS_student.id'], name='FK_KURS_enrollments_students'), PrimaryKeyConstraint('id', name='PK_KURS_enrollments'), Index('UQ_KURS_enrollments_student_course', 'student_id', 'course_id', mssql_clustered=False, unique=True))
    id: Mapped[int] = mapped_column(Integer, Identity(start=1, increment=1), primary_key=True, autoincrement=True)
    student_id: Mapped[int] = mapped_column(Integer, nullable=False)
    course_id: Mapped[int] = mapped_column(Integer, nullable=False)
    enrolled_at: Mapped[datetime.datetime] = mapped_column(DateTime, nullable=False, server_default=text('(getdate())'))
    status: Mapped[str] = mapped_column(Unicode(20, 'Cyrillic_General_CI_AS'), nullable=False, server_default=text("(N'активен')"))
    course: Mapped['KURSCourse'] = relationship('KURSCourse', back_populates='KURS_enrollments')
    student: Mapped['KURSStudent'] = relationship('KURSStudent', back_populates='KURS_enrollments')
    KURS_results: Mapped[list['KURSResults']] = relationship('KURSResults', back_populates='enrollment')

class KURSMaterials(Base):
    __tablename__ = 'KURS_materials'
    __table_args__ = (ForeignKeyConstraint(['course_id'], ['KURS_course.id'], name='FK_materials_courses'), PrimaryKeyConstraint('id', name='PK_materials'), Index('UQ_materials_course_title', 'course_id', 'title', mssql_clustered=False, unique=True))
    id: Mapped[int] = mapped_column(Integer, Identity(start=1, increment=1), primary_key=True, autoincrement=True)
    course_id: Mapped[int] = mapped_column(Integer, nullable=False)
    type: Mapped[str] = mapped_column(Unicode(20, 'Cyrillic_General_CI_AS'), nullable=False)
    title: Mapped[str] = mapped_column(Unicode(200, 'Cyrillic_General_CI_AS'), nullable=False)
    link: Mapped[Optional[str]] = mapped_column(Unicode(500, 'Cyrillic_General_CI_AS'))
    course: Mapped['KURSCourse'] = relationship('KURSCourse', back_populates='KURS_materials')
    KURS_results: Mapped[list['KURSResults']] = relationship('KURSResults', back_populates='material')

class KURSResults(Base):
    __tablename__ = 'KURS_results'
    __table_args__ = (ForeignKeyConstraint(['enrollment_id'], ['KURS_enrollments.id'], name='FK_results_enrollments'), ForeignKeyConstraint(['material_id'], ['KURS_materials.id'], name='FK_results_materials'), PrimaryKeyConstraint('id', name='PK_results'), Index('UQ_results_enrollment_material', 'enrollment_id', 'material_id', mssql_clustered=False, unique=True))
    id: Mapped[int] = mapped_column(Integer, Identity(start=1, increment=1), primary_key=True, autoincrement=True)
    enrollment_id: Mapped[int] = mapped_column(Integer, nullable=False)
    material_id: Mapped[int] = mapped_column(Integer, nullable=False)
    grade: Mapped[int] = mapped_column(Integer, nullable=False)
    date: Mapped[datetime.date] = mapped_column(Date, nullable=False, server_default=text('(getdate())'))
    comment: Mapped[Optional[str]] = mapped_column(Unicode(300, 'Cyrillic_General_CI_AS'))
    enrollment: Mapped['KURSEnrollments'] = relationship('KURSEnrollments', back_populates='KURS_results')
    material: Mapped['KURSMaterials'] = relationship('KURSMaterials', back_populates='KURS_results')