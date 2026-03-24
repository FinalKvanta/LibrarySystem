# LibrarySystem — Полноценная серверная часть CoreWCF-сервиса

## Скриншоты
<img width="1097" height="269" alt="image" src="https://github.com/user-attachments/assets/53230150-a6a9-4fff-a71b-b1c783dbfd94" />




## Описание

Система управления библиотекой с многослойной архитектурой (Clean Architecture), JWT-аутентификацией, ролевой авторизацией, EF Core (InMemory), Serilog-логированием, модульными тестами и поддержкой HTTP/TCP транспортов.

## Архитектура (Clean Architecture)

```
LibrarySystem/
├── src/
│   ├── LibrarySystem.Core/              # Domain Layer
│   │   ├── Entities/                    # Book, Reader, Loan, User
│   │   ├── Enums/                       # UserRole, LoanStatus
│   │   ├── Interfaces/                  # IBookRepository, IReaderRepository, etc.
│   │   └── Exceptions/                  # DomainException, NotFoundException, AccessDeniedException
│   │
│   ├── LibrarySystem.Application/       # Application Layer
│   │   ├── DTOs/                        # BookDto, ReaderDto, LoanDto, AuthTokenDto, etc.
│   │   ├── Contracts/                   # ILibraryService (ServiceContract)
│   │   ├── Services/                    # AuthService, BookService, ReaderService, LoanService, StatsService
│   │   └── Validators/                  # BookValidator
│   │
│   ├── LibrarySystem.Infrastructure/    # Infrastructure Layer
│   │   ├── Data/                        # LibraryDbContext, SeedData
│   │   └── Repositories/               # BookRepository, ReaderRepository, LoanRepository, etc.
│   │
│   ├── LibrarySystem.Shared/            # Shared Utilities
│   │   ├── Helpers/                     # PasswordHelper (SHA256)
│   │   └── Extensions/                  # DateTimeExtensions
│   │
│   ├── LibrarySystem.API/              # Presentation Layer (CoreWCF)
│   │   ├── Services/                    # LibraryServiceImpl (SOAP endpoint)
│   │   └── Program.cs                  # Host configuration
│   │
│   └── LibrarySystem.Client/           # Test Client
│       └── Program.cs                  # Comprehensive test scenarios
│
└── tests/
    └── LibrarySystem.Tests/            # Unit Tests (xUnit)
        ├── BookServiceTests.cs         # 7 tests
        ├── LoanServiceTests.cs         # 7 tests
        └── AuthServiceTests.cs         # 8 tests
```

## Контракт службы (16 операций)

```csharp
[ServiceContract]
public interface ILibraryService
{
    // Аутентификация
    AuthTokenDto Authenticate(string username, string password);
    AuthTokenDto RefreshToken(string token);

    // Книги (CRUD + поиск)
    BookDto AddBook(string token, string title, string author, string isbn, int year, string genre, int totalCopies);
    BookDto GetBook(string token, int bookId);
    List<BookDto> GetAllBooks(string token);
    List<BookDto> SearchBooks(string token, SearchCriteriaDto criteria);
    bool UpdateBook(string token, int bookId, string title, string author, string genre, int totalCopies);
    bool DeleteBook(string token, int bookId);

    // Читатели
    ReaderDto RegisterReader(string token, string fullName, string email, string phone);
    ReaderDto GetReader(string token, int readerId);
    List<ReaderDto> GetAllReaders(string token);

    // Выдача/возврат
    LoanDto LendBook(string token, int bookId, int readerId, int daysToReturn);
    bool ReturnBook(string token, int loanId);
    List<LoanDto> GetReaderLoans(string token, int readerId);
    List<LoanDto> GetOverdueLoans(string token);

    // Статистика
    LibraryStatsDto GetStatistics(string token);
}
```

## Безопасность

### JWT-аутентификация (HMAC-SHA256)
- Токен генерируется при вызове `Authenticate()`
- Формат: `base64(header).base64(payload).base64(signature)`
- Время жизни: 2 часа
- Пароли хранятся в виде SHA256-хэшей

### Ролевая авторизация

| Операция | Reader | Librarian | Admin |
|----------|:------:|:---------:|:-----:|
| Authenticate | + | + | + |
| GetAllBooks, GetBook, SearchBooks | + | + | + |
| GetReaderLoans | + | + | + |
| AddBook, UpdateBook | - | + | + |
| RegisterReader, GetReader, GetAllReaders | - | + | + |
| LendBook, ReturnBook, GetOverdueLoans | - | + | + |
| GetStatistics | - | + | + |
| DeleteBook | - | - | + |

### Пользователи

| Логин | Пароль | Роль |
|-------|--------|------|
| admin | admin123 | Admin |
| librarian | lib123 | Librarian |
| reader | read123 | Reader |

## Технологии

| Компонент | Технология |
|-----------|-----------|
| Фреймворк | .NET 8, ASP.NET Core |
| SOAP | CoreWCF 1.8 (BasicHttpBinding + NetTcpBinding) |
| ORM | Entity Framework Core (InMemory) |
| Логирование | Serilog (Console + File) |
| Тестирование | xUnit (22 теста) |
| Безопасность | Custom JWT (HMAC-SHA256) |

## Эндпоинты

| Протокол | Адрес |
|----------|-------|
| HTTP | `http://localhost:5000/LibraryService/http` |
| TCP | `net.tcp://localhost:8090/LibraryService/tcp` |
| WSDL | `http://localhost:5000/LibraryService/http?wsdl` |

## Как запустить

```bash
# Сборка
dotnet build

# Тесты (22 теста)
dotnet test

# Запуск сервиса
dotnet run --project src/LibrarySystem.API

# Запуск клиента (в другом терминале)
dotnet run --project src/LibrarySystem.Client
```

## Тесты (22 теста)

| Класс | Количество | Описание |
|-------|:----------:|----------|
| BookServiceTests | 7 | CRUD операции, поиск, валидация |
| LoanServiceTests | 7 | Выдача, возврат, просрочка, проверка доступности |
| AuthServiceTests | 8 | Аутентификация, валидация токена, роли |

## Скриншоты работы

### Запуск сервиса
```
[11:59:58 INF] Starting LibrarySystem API...
[11:59:58 INF] Database seeded with initial data
[11:59:58 INF] LibrarySystem API is running
[11:59:58 INF] HTTP: http://localhost:5000/LibraryService/http
[11:59:58 INF] TCP: net.tcp://localhost:8090/LibraryService/tcp
[11:59:58 INF] Now listening on: http://0.0.0.0:8090
[11:59:58 INF] Now listening on: http://localhost:5000
```

### Тестовый клиент (HTTP)
```
=== LibrarySystem WCF Client ===

>>> Testing via BasicHttpBinding (HTTP) <<<

--- Authentication ---
Authenticating as admin... OK. Token received, role: Admin
Authenticating as librarian... OK. Token received, role: Librarian
Authenticating as reader... OK. Token received, role: Reader
Testing invalid credentials... OK. Got expected error.

--- Books ---
Getting all books... OK. Found 5 books.
Getting book by ID (1)... OK. Title: War and Peace, Author: Leo Tolstoy
Searching books by author 'Tolstoy'... OK. Found 2 books.
Adding a new book... OK. Added book ID: 6
Updating book... OK.
Deleting book (admin only)... OK.

--- Readers ---
Getting all readers... OK. Found 3 readers.
Registering new reader... OK. ID: 4, Name: Natalia Romanova

--- Loans ---
Lending book... OK. Loan ID: 3, Due: 2026-04-06
Getting reader loans... OK. Found 2 loans.
Getting overdue loans... OK. Found 1 overdue.
Returning book... OK.

--- Statistics ---
Total books: 5, Total readers: 4, Active loans: 2, Overdue: 1

--- Access Control ---
Reader trying to add book... ACCESS DENIED (expected)
Reader trying to get readers... ACCESS DENIED (expected)
Librarian trying to delete... ACCESS DENIED (expected)
Reader can view books... OK. Reader sees 5 books.
```

### Тестовый клиент (TCP)
```
>>> Testing via NetTcpBinding (TCP) <<<

--- Authentication ---
Authenticating as admin... OK.
Authenticating as librarian... OK.

--- Books ---
Getting all books... OK. Found 5 books.
Adding a new book... OK. Added book ID: 7
Deleting book... OK.

--- Loans ---
Lending book... OK. Loan ID: 4
Returning book... OK.

--- Access Control ---
Reader trying to add book... ACCESS DENIED (expected)
Librarian trying to delete... ACCESS DENIED (expected)

=== All tests completed ===
```

### Тесты (22/22 passed)
```
dotnet test
Пройден! : не пройдено 0, пройдено 22, пропущено 0, всего 22
```
