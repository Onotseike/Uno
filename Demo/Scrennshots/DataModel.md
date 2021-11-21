# Data Model

``` mermaid
erDiagram
    ACCOUNT {
        Guid Id
        string BankName
        string Holder
        string Number
        string Iban
        string RoutingNumber
        string SwiftNumber
        string Currency
        bool IsUser

        Guid ClientId
    }

    ADDRESS{
        Guid Id
        string AddressOne
        string AddressTwo
        string City
        string State
        string Country
        string PostalCode
        string IsUser
        AddressType Type

        Guid AccountId
        Guid ClientId
    }

    CLIENT{
        Guid Id
        ClientType Type
        string Name
    }

    COMMUNICATION{
        Guid Id
        string HomeEmail
        string WorkEmail
        string HomePhone
        string WorkPhone
        string Website
        bool IsUser

        Guid ClientId
    }

    INVOICE{
        Guid Id
        string Currency
        DateTime IssueDate
        DateTime DueDate
        InvoiceStatus Status
        string FullName

        Guid UserBankAccountId FK
        Guid ClientId
        Guid UserAddressId

        ItemBlobArray Items
    }

    ItemBlob{
        string ItemType
        string Description
        double Price
    }

    INVOICE ||--|{ ItemBlob : contains
    CLIENT ||--o{ INVOICE: "liable for"
    ADDRESS ||--o{ INVOICE : "issued from"
    ACCOUNT ||--o{ INVOICE : "paid to"

    CLIENT ||..|| COMMUNICATION : has
    CLIENT ||..|| ADDRESS : has
    CLIENT ||..|| ACCOUNT : has

    ACCOUNT ||..|| ADDRESS : has

```
