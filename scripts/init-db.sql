-- Initialize PostgreSQL database for Mo5 RAG Server
-- This script is executed when the PostgreSQL container starts for the first time

-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Verify pgvector is installed
SELECT extname, extversion FROM pg_extension WHERE extname = 'vector';

-- Create indexes for better performance (will be created by EF migrations, but good to have as backup)
-- These will be created by Entity Framework migrations, but we ensure the extension is available

-- Log successful initialization
DO $$
BEGIN
    RAISE NOTICE 'Mo5 RAG Server database initialized successfully with pgvector extension';
END $$;
