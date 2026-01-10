# Database Schema

Database: PostgreSQL  
Application: ASP.NET (EF Core)  
Timezone: UTC  
Naming: snake_case

---

## Table: users

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| name | text | yes | |
| email | text | no | UNIQUE |
| password_hash | text | yes | |
| phone | text | yes | |
| created_at | timestamptz | no | |
| last_seen | timestamptz | yes | |

---

## Table: devices

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| device_key | text | no | UNIQUE |
| platform | text | no | |
| created_at | timestamptz | no | |

---

## Table: users_devices

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| user_id | bigint | no | PK, FK → users.id |
| device_id | bigint | no | PK, FK → devices.id |
| linked_at | timestamptz | no | |

---

## Table: sessions

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id |
| device_id | bigint | no | FK → devices.id |
| refresh_token_hash | text | no | |
| created_at | timestamptz | no | |
| expires_at | timestamptz | no | |
| revoked_at | timestamptz | yes | |

---

## Table: authidentities

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id |
| loginprovider | text | no | UNIQUE (loginprovider, providerkey) |
| providerkey | text | no | UNIQUE (loginprovider, providerkey) |

---

## Table: modules

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| name | text | no | |
| description | text | yes | |
| interval_count | integer | no | |

---

## Table: products

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| product_id | bigint | no | PK |
| module_id | bigint | no | FK → modules.id |
| name | text | no | |

---

## Table: purchases

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id |
| product_id | bigint | no | FK → products.product_id |
| amount_cents | integer | no | |
| provider | text | no | |
| receipt | text | yes | |
| status | text | no | |
| provider_payment_id | text | no | UNIQUE (provider, provider_payment_id) |
| days | integer | no | |
| currency | char(3) | no | |
| created_at | timestamptz | no | |
| purchased_at | timestamptz | yes | |

---

## Table: adminaccesshistory

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| admin_id | bigint | no | FK → users.id |
| target_user_id | bigint | no | FK → users.id |
| product_id | bigint | no | FK → products.product_id |
| days | integer | no | |
| created_at | timestamptz | no | |

---

## Table: promocodes

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| code | text | no | UNIQUE |
| max_usage_count | integer | no | |
| days | integer | no | |
| product_id | bigint | no | FK → products.product_id |

---

## Table: promocodeusages

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id |
| code_id | bigint | no | FK → promocodes.id |
| used_at | timestamptz | no | |

---

## Table: usermoduleaccess

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | UNIQUE (user_id, module_id), FK → users.id |
| module_id | bigint | no | UNIQUE (user_id, module_id), FK → modules.id |
| ends_at | timestamptz | no | |

---

## Table: progress

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id |
| lesson_id | bigint | no | |
| result_id | bigint | no | |
