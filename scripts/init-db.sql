-- Database initialization script for development environment

-- Create additional databases if needed
CREATE DATABASE IF NOT EXISTS ecommerce_test;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create indexes for better performance
-- These will be created by EF Core migrations, but having them here as reference

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE ecommerce TO postgres;
GRANT ALL PRIVILEGES ON DATABASE ecommerce_dev TO postgres;
GRANT ALL PRIVILEGES ON DATABASE ecommerce_test TO postgres;

-- Create read-only user for reporting
CREATE USER IF NOT EXISTS ecommerce_readonly WITH PASSWORD 'readonly123';
GRANT CONNECT ON DATABASE ecommerce TO ecommerce_readonly;
GRANT CONNECT ON DATABASE ecommerce_dev TO ecommerce_readonly;
GRANT USAGE ON SCHEMA public TO ecommerce_readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO ecommerce_readonly;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO ecommerce_readonly;