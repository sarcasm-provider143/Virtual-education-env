Инструкция для запуска API:
1. Открыть проект VirtualEduAPI в Pycharm
2. Открыть консоль проекта
3. В консоль ввести команду для инициализации виртуальной среды: python -m venv .venv
4. Ввести команду для активации среды: .\.venv\Scripts\Activate.ps1
5. Если нужно - указать прокси (для компьютера в колледже): $env:HTTP_PROXY="http://proxy.ygk.ru:8080"
6. Установить все необходимые библиотеки: pip install fastapi uvicorn sqlalchemy pyodbc python-jose passlib python-multipart PyJWT
7. Запустить сервер: uvicorn main:app --reload --port 8001
8. Открыть ссылку из консоли в браузере
