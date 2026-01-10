# Database Schema

Database: PostgreSQL  
Application: ASP.NET (EF Core)  
Timezone: UTC  
Naming: snake_case  

## Table: users

### Columns
| Name | Type | Nullable | Constraints |
|-----|-----|----------|------------|
| id | bigint | no | PK |
| email | text | no | UNIQUE |
| password_hash | text | yes | |
| created_at | timestamptz | no | |

### Primary Key
- id

### Unique Constraints
- UNIQUE (email)