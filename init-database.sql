-- PostgreSQL runs this script automatically on first startup.
-- Creates a separate database for each service — services are fully isolated
-- and can never accidentally query each other's tables.

CREATE DATABASE orders_db;
CREATE DATABASE inventory_db;
CREATE DATABASE notifications_db;