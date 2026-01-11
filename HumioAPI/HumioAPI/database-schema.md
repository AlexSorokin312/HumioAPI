# Database Schema

**Database:** PostgreSQL  
**Application:** ASP.NET Core (EF Core + ASP.NET Core Identity)  
**Timezone:** UTC  
**Naming:** snake_case

Подключение к бд: psql -h 185.28.84.191 -p 5432 -U postgres -d Humio
Пароль: <SECRET>
---

## Table: users (AspNetUsers)

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| name | text | yes | |
| user_name | text | yes | |
| normalized_user_name | text | yes | UNIQUE |
| email | text | no | UNIQUE |
| normalized_email | text | yes | UNIQUE |
| email_confirmed | boolean | no | |
| password_hash | text | yes | |
| security_stamp | text | yes | |
| concurrency_stamp | text | yes | |
| phone_number | text | yes | |
| phone_number_confirmed | boolean | no | |
| two_factor_enabled | boolean | no | |
| lockout_end | timestamptz | yes | |
| lockout_enabled | boolean | no | |
| access_failed_count | integer | no | |
| created_at | timestamptz | no | DEFAULT now() |
| last_seen | timestamptz | yes | |

---

## Table: user_profiles

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| user_id | bigint | no | PK, FK → users.id |
| first_name | text | yes | |
| last_name | text | yes | |
| middle_name | text | yes | |
| birth_date | date | yes | |
| city | text | yes | |
| gender | text | yes | |

---

## Table: roles (AspNetRoles)

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| name | text | yes | |
| normalized_name | text | yes | UNIQUE |
| concurrency_stamp | text | yes | |

---

## Table: user_roles (AspNetUserRoles)

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| user_id | bigint | no | PK, FK → users.id |
| role_id | bigint | no | PK, FK → roles.id |

---

## Table: user_claims (AspNetUserClaims)

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id |
| claim_type | text | yes | |
| claim_value | text | yes | |

---

## Table: role_claims (AspNetRoleClaims)

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| role_id | bigint | no | FK → roles.id |
| claim_type | text | yes | |
| claim_value | text | yes | |

---

## Table: user_logins (AspNetUserLogins)

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| login_provider | text | no | PK |
| provider_key | text | no | PK |
| provider_display_name | text | yes | |
| user_id | bigint | no | FK → users.id |

---

## Table: user_tokens (AspNetUserTokens)

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| user_id | bigint | no | PK, FK → users.id |
| login_provider | text | no | PK |
| name | text | no | PK |
| value | text | yes | |

---

## Table: devices

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| device_key | text | no | UNIQUE |
| platform | text | no | |
| created_at | timestamptz | no | DEFAULT now() |

---

## Table: users_devices

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| user_id | bigint | no | PK, FK → users.id |
| device_id | bigint | no | PK, FK → devices.id |
| linked_at | timestamptz | no | DEFAULT now() |
| revoked_at | timestamptz | yes | |

---

## Table: modules

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| name | text | no | |
| description | text | yes | |
| interval_count | integer | no | CHECK (interval_count > 0) |

---

## Table: products

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| module_id | bigint | no | FK → modules.id, INDEX |
| name | text | no | |

---

## Table: purchases

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id, INDEX |
| product_id | bigint | no | FK → products.id |
| amount_cents | integer | no | CHECK (amount_cents >= 0) |
| currency | char(3) | no | CHECK (currency ~ '^[A-Z]{3}$') |
| provider | text | no | |
| provider_payment_id | text | no | UNIQUE (provider, provider_payment_id) |
| receipt | text | yes | |
| status | text | no | CHECK (status IN ('pending','paid','failed','refunded')) |
| days | integer | no | CHECK (days > 0) |
| created_at | timestamptz | no | DEFAULT now() |
| purchased_at | timestamptz | yes | SET BY TRIGGER WHEN status = 'paid' |

---

## Table: admin_access_history

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| admin_id | bigint | no | FK → users.id |
| target_user_id | bigint | no | FK → users.id |
| product_id | bigint | no | FK → products.id |
| days | integer | no | CHECK (days > 0) |
| created_at | timestamptz | no | DEFAULT now() |

---

## Table: promocodes

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| code | text | no | UNIQUE |
| max_usage_count | integer | no | CHECK (max_usage_count > 0) |
| days | integer | no | CHECK (days > 0) |
| product_id | bigint | no | FK → products.id |

---

## Table: promocode_usages

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| user_id | bigint | no | PK, FK → users.id, INDEX |
| promocode_id | bigint | no | PK, FK → promocodes.id |
| used_at | timestamptz | no | DEFAULT now() |

---

## Table: user_module_access

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| user_id | bigint | no | PK, FK → users.id, INDEX |
| module_id | bigint | no | PK, FK → modules.id, INDEX |
| ends_at | timestamptz | no | |

---

