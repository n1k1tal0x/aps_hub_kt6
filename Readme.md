я не сразу понял, что в презе прямой SQL запрос, но это говно, поэтому всё на объетках

# Запуск проекта

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

# Задания

### Задание 1

POST http://localhost:5197/api/User
BODY:
```
{
    "Age":18,
    "Name":"Lorem Name",
    "Email":"Lorem Main"
}
```

В бд теперь есть пользователь с возврастом 

### Задание 2

POST http://localhost:5197/api/UpdateEmail?SearchName=Lorem Name
Body:
```
{
    "newEmail": "newEmail@gmail.com"
}

### Задание 3

POST http://localhost:5197/api/transacrtions/User
BODY:
```
{
    "Age":18,
    "Name":"Lorem Name",
    "Email":"Lorem Main"
}
```
если возраст 18 и больше, то транзакция пройдёт, если меньше, то откат транзакции и вернёт bad request
```