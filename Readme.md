1. Запустить докер
postgres в docker-compose

2. Сделать миграцию
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

3. Если ef не найден
```
dotnet tool install --global dotnet-ef
```