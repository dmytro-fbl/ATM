# ATM System (Система управління банкоматом)

[![.NET](https://img.shields.io/badge/.NET-8.0%2F9.0-blueviolet)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/ORM-EF%20Core-blue)](https://learn.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Проєкт представляє собою **Backend-частину (Web API)** для програмного забезпечення банкомата. Система дозволяє керувати рахунками, обробляти транзакції, видавати готівку з урахуванням наявності купюр у касетах та вести аудит усіх операцій.

---

## Як запустити локально

1. **Склонуйте репозиторій:**
   ```bash
   git clone [посилання-на-ваш-репозиторій]
Відкрийте рішення (ATM_System.sln) у Visual Studio.

Встановіть залежності: Переконайтеся, що встановлено NuGet-пакет BCrypt.Net-Next для роботи системи безпеки.

Налаштуйте базу даних:

Переконайтеся, що у вас встановлено SQL Server LocalDB.

Відкрийте Package Manager Console, оберіть Default project: ATM.Infrastructure.

Виконайте команду:

PowerShell
Update-Database
Запустіть проєкт: Оберіть ATM.API як запускний проєкт та натисніть F5.

Programming Principles (Принципи програмування)
У цьому проєкті дотримано ключових принципів розробки:

SRP (Single Responsibility): Кожен клас має чітку зону відповідальності (репозиторії - дані, сервіси - логіка).

DIP (Dependency Inversion): Високорівневі модулі залежать від інтерфейсів, а не від реалізацій.

DRY (Don't Repeat Yourself): Спільна логіка валідації винесена в приватні хелпер-методи.

Fail Fast: Система миттєво викидає Exception, якщо виявлено невідповідність даних (невірний ПІН, недостатній баланс).

Encapsulation: Складна логіка підбору купюр прихована всередині AtmService.

Design Patterns (Патерни проєктування)
Repository Pattern: Ізоляція EF Core від бізнес-логіки для полегшення тестування.

Service Layer: Центральна ланка (AtmService), яка координує роботу репозиторіїв.

Strategy Pattern (Greedy Algorithm): Реалізовано в алгоритмі видачі готівки для оптимального підбору банкнот.

Dependency Injection (DI): Всі залежності реєструються в Program.cs та інжектуються через конструктори.

Security (Безпека)
BCrypt Hashing: ПІН-коди зберігаються виключно у вигляді хешів".

Data Integrity: Структура БД гарантує цілісність зв'язків: User -> Account -> Card.

Refactoring Techniques (Техніки рефакторингу)
Extract Method: Повторювана валідація винесена в методи GetCardAsync та GetAccountAsync.

Move Field: Зв'язок перенесено з рахунку на картку для підтримки логіки "багато карток на один рахунок".

Rename Field: Приведення назв до стандарту (напр., Pin -> PinHash).

Extract Interface: Створення абстракцій для всіх сервісів та репозиторіїв.

Технологічний стек
Платформа: .NET 8 / .NET 9

ORM: Entity Framework Core

База даних: MS SQL Server

Безпека: BCrypt.Net-Next

Архітектура: Layered Architecture
