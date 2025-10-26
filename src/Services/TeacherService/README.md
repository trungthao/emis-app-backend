Teacher Service
===============

This is a minimal Teacher Service scaffold (Domain / Application / Infrastructure / API) to manage teachers.

How to run locally (quick):

1. Ensure the teacher MySQL database is running (docker-compose includes `teacherdb` on port 3307).
2. From `src/Services/TeacherService/Teacher.API` run:

```bash
dotnet restore
dotnet build
dotnet run
```

The API exposes Swagger at `http://localhost:5002/swagger` (default port can be configured in `appsettings.json`).

This scaffold includes basic CreateTeacher command and repository implementation. Extend with queries, handlers and controllers as needed.
