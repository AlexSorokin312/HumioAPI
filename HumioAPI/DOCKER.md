# Docker Setup

## 1. Prepare env file

Create `.env` from template:

```powershell
Copy-Item .env.example .env
```

Then update secrets in `.env` (`JWT_KEY`, Google and SMTP credentials).

## 2. Build and start all containers

```powershell
docker compose up --build -d
```

Services:
- API: `http://localhost:8080`
- Swagger (Development): `http://localhost:8080/swagger`
- PostgreSQL: `localhost:5432`
- pgAdmin: `http://localhost:5050`

## 3. pgAdmin login

- Email: value from `PGADMIN_DEFAULT_EMAIL`
- Password: value from `PGADMIN_DEFAULT_PASSWORD`

The server `Humio PostgreSQL` is preconfigured in `pgadmin/servers.json`.

## 4. Stop stack

```powershell
docker compose down
```

To stop and remove volumes:

```powershell
docker compose down -v
```
