# Diagrammes de Flux

## Flux global de l'application

```mermaid
flowchart LR
    subgraph "Client"
        Browser[Navigateur / Postman]
    end

    subgraph "API Layer"
        Swagger[Swagger UI]
        Controllers[Controllers REST]
        Middleware[Exception Middleware]
    end

    subgraph "Application Layer"
        Services[Services]
        DTOs[DTOs In/Out]
    end

    subgraph "Domain Layer"
        Entities[Entités Métier]
        Rules[Règles de Gestion]
        Interfaces[Interfaces Repositories]
    end

    subgraph "Infrastructure Layer"
        Repos[Repositories EF]
        DbCtx[AppDbContext]
        DB[(Base de données)]
    end

    Browser -->|HTTP| Swagger
    Browser -->|HTTP| Controllers
    Controllers -->|Appel| Services
    Services -->|Utilise| DTOs
    Services -->|Appelle| Entities
    Entities -->|Valide| Rules
    Services -->|Via interface| Interfaces
    Repos -->|Implémente| Interfaces
    Repos -->|Utilise| DbCtx
    DbCtx -->|Lit/Écrit| DB
    Middleware -.->|Intercepte erreurs| Controllers

    style Entities fill:#4CAF50,color:#fff
    style Rules fill:#4CAF50,color:#fff
    style DB fill:#FF9800,color:#fff
```

## Flux de création d'une commande complète

Ce diagramme montre l'enchaînement complet des appels API pour créer, peupler et finaliser une commande.

```mermaid
sequenceDiagram
    actor User as Utilisateur
    participant API as API REST
    participant App as Application
    participant Dom as Domain
    participant Infra as Infrastructure

    Note over User,Infra: Étape 1 - Créer la commande
    User->>API: POST /api/orders {customerId}
    API->>App: OrderService.CreateOrder()
    App->>Infra: CustomerRepo.GetById()
    Infra-->>App: Customer ✅
    App->>Dom: new Order(id, customerId)
    Dom-->>App: Order [Pending]
    App->>Infra: OrderRepo.Add(order)
    Infra-->>App: OK
    App-->>API: Order créée
    API-->>User: 201 Created + Order

    Note over User,Infra: Étape 2 - Ajouter des articles
    User->>API: POST /api/orders/{id}/items {productId, qty}
    API->>App: OrderService.AddItemToOrder()
    App->>Infra: OrderRepo.GetById()
    Infra-->>App: Order
    App->>Infra: ProductRepo.GetById()
    Infra-->>App: Product (avec prix actuel)
    App->>Dom: order.AddItem(productId, qty, price)
    Dom->>Dom: Vérifie statut Pending ✅
    Dom->>Dom: Vérifie produit pas déjà présent ✅
    Dom-->>App: OrderItem ajouté
    App->>Infra: OrderRepo.Save(order)
    API-->>User: 204 No Content

    Note over User,Infra: Étape 3 - Confirmer
    User->>API: PUT /api/orders/{id}/confirm
    API->>App: OrderService.ConfirmOrder()
    App->>Infra: OrderRepo.GetById()
    Infra-->>App: Order [Pending, 1+ items]
    App->>Dom: order.Confirm()
    Dom->>Dom: Vérifie statut Pending ✅
    Dom->>Dom: Vérifie items non vide ✅
    Dom-->>App: Status → Confirmed
    App->>Infra: OrderRepo.Save(order)
    API-->>User: 204 No Content
```

## Flux de gestion des erreurs

```mermaid
flowchart TD
    A[Requête HTTP entrante] --> B[Controller]
    B --> C[Service Application]
    C --> D{Ressource trouvée ?}

    D -->|Non| E[ApplicationServiceException 404]
    E --> F[Middleware intercepte]
    F --> G["HTTP 404 {title, detail}"]

    D -->|Oui| H[Entité Domain]
    H --> I{Règle métier OK ?}

    I -->|Non| J[DomainException]
    J --> K[Middleware intercepte]
    K --> L["HTTP 400 {title, detail}"]

    I -->|Oui| M[Repository Infrastructure]
    M --> N{Base de données OK ?}

    N -->|Non| O[InfrastructureException]
    O --> P[Middleware intercepte]
    P --> Q["HTTP 500 {error}"]

    N -->|Oui| R[Sauvegarde réussie]
    R --> S[HTTP 200/201/204]

    style G fill:#FF9800,color:#fff
    style L fill:#f44336,color:#fff
    style Q fill:#9C27B0,color:#fff
    style S fill:#4CAF50,color:#fff
```

## Flux d'injection de dépendances

Ce diagramme montre comment Program.cs câble les dépendances au démarrage.

```mermaid
flowchart TD
    subgraph "Program.cs - Enregistrement"
        A[AddDbContext AppDbContext]
        B[AddScoped ProductService]
        C[AddScoped CustomerService]
        D[AddScoped SupplierService]
        E[AddScoped OrderService]
        F["AddScoped IProductRepository → EfProductRepository"]
        G["AddScoped ICustomerRepository → EfCustomerRepository"]
        H["AddScoped ISupplierRepository → EfSupplierRepository"]
        I["AddScoped IOrderRepository → EfOrderRepository"]
    end

    subgraph "À chaque requête HTTP"
        J[Nouvelle instance ProductService]
        K[Nouvelle instance EfProductRepository]
        L[Instance partagée AppDbContext]
    end

    A --> L
    B --> J
    F --> K
    J -->|"reçoit via constructeur"| K
    K -->|"reçoit via constructeur"| L

    style A fill:#FF9800,color:#fff
    style L fill:#FF9800,color:#fff
```
