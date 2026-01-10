# Database Schema

**Database:** PostgreSQL  
**Application:** ASP.NET Core (EF Core + ASP.NET Core Identity)  
**Timezone:** UTC  
**Naming:** snake_case

Подключение к бд: psql -h 185.28.84.191 -p 5432 -U postgres -d Humio
Пароль: _qNJk1kt47KmcpqG_1QZk-OOitYIzJ
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
| created_at | timestamptz | no | |
| last_seen | timestamptz | yes | |

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
| created_at | timestamptz | no | |

---

## Table: users_devices

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| user_id | bigint | no | PK, FK → users.id |
| device_id | bigint | no | PK, FK → devices.id |
| linked_at | timestamptz | no | |
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
| module_id | bigint | no | FK → modules.id |
| name | text | no | |

---

## Table: purchases

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id |
| product_id | bigint | no | FK → products.id |
| amount_cents | integer | no | CHECK (amount_cents >= 0) |
| currency | char(3) | no | |
| provider | text | no | |
| provider_payment_id | text | no | UNIQUE (provider, provider_payment_id) |
| receipt | text | yes | |
| status | text | no | CHECK (status IN ('pending','paid','failed','refunded')) |
| days | integer | no | CHECK (days > 0) |
| created_at | timestamptz | no | |
| purchased_at | timestamptz | yes | |

---

## Table: admin_access_history

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| admin_id | bigint | no | FK → users.id |
| target_user_id | bigint | no | FK → users.id |
| product_id | bigint | no | FK → products.id |
| days | integer | no | CHECK (days > 0) |
| created_at | timestamptz | no | |

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
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id, UNIQUE (user_id, promocode_id) |
| promocode_id | bigint | no | FK → promocodes.id, UNIQUE (user_id, promocode_id) |
| used_at | timestamptz | no | |

---

## Table: user_module_access

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id, UNIQUE (user_id, module_id) |
| module_id | bigint | no | FK → modules.id, UNIQUE (user_id, module_id) |
| ends_at | timestamptz | no | |

---

## Table: progress

| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| user_id | bigint | no | FK → users.id, UNIQUE (user_id, lesson_id) |
| lesson_id | bigint | no | UNIQUE (user_id, lesson_id) |
| result | text | no | |
| updated_at | timestamptz | no | |
