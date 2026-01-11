# 🧠 Retrieval-Augmented Generation server - MO5 Knowledge API

## 📌 Overview

This project aims to provide an **AI-powered knowledge base and development assistant** for building software in **C and Assembly for the Thomson MO5** (retro computing platform).

The goal is to **enhance developer productivity** by allowing AI tools (such as GitHub Copilot or ChatGPT) to have **full contextual knowledge** about the MO5 architecture, including:

- Motorola 6809 CPU instructions and quirks
- Memory layout and I/O mapping
- ROM/RAM organization, video memory, and hardware registers
- Development toolchain setup, build formats, and best practices

This will make it easier for anyone to build MO5 applications while benefiting from **AI-assisted coding with domain-specific knowledge**.

## 🚀 Quick Start

**Choose your deployment environment:**

- **[Raspberry Pi NAS](deployment/pi-nas/README.md)** - Optimized for ARM64 (Raspberry Pi 4/5)
- **[Azure Cloud](deployment/azure/README.md)** - Production deployment on Azure Container Instances
- **[Local Development](deployment/local-dev/README.md)** - Docker Compose for local development
- **[Portainer](deployment/portainer/README.md)** - Web UI for Docker management

See the **[Deployment Guide](deployment/README.md)** for detailed instructions.

---

## 🌐 Public RAG Server Available

The MO5 RAG server is now **publicly deployed and accessible on the Internet**.

👉 https://retrocomputing-ai.cloud/

This means that **you no longer need to host your own RAG server** to use the MCP server or benefit from AI-assisted MO5 development.

The public instance provides:
- semantic search over the MO5 knowledge base
- continuously updated documentation
- a stable API endpoint usable from coding agents

If you just want to **use the MCP server**, you can directly point it to the public RAG instance and get started immediately.

Self-hosting the RAG server is still possible if you want to:
- experiment with the internals
- modify the ingestion pipeline
- run everything offline
- or customize the knowledge base

But for most users, the hosted version should be sufficient.

---

## ⚖️ Solutions Considered

### 1. `.ai.md` or Static Context Files
- **Pros**: Simple to create and version; no infrastructure required
- **Cons**: Static, not easily searchable or updatable; hard to maintain as knowledge grows

### 2. Embedding the Knowledge Base Directly into AI Prompts
- **Pros**: No backend needed; quick to prototype
- **Cons**: Context size is very limited; not scalable; not reusable by others

### 3. **(Chosen) Knowledge API + Plugins (RAG Architecture)**
- **Pros**:
  - Centralized and reusable for any developer
  - Dynamic and easily updateable knowledge base
  - Allows semantic search and fine-grained answers
  - Can be plugged into multiple IDEs or ChatGPT custom GPTs
- **Cons**:
  - Requires hosting and initial setup
  - More complex than static files

✅ **This is the chosen approach.**

---

## 🧩 Architecture Overview

The solution will be based on a **Retrieval-Augmented Generation (RAG)** pattern:

1. **Knowledge ingestion**
   - Collect MO5-related documents (text, manuals, code samples, PDFs)
   - Split them into chunks and store them with vector embeddings in a vector database

2. **Knowledge API**
   - Provide a REST API that receives a user query, performs a semantic search on the knowledge base, and returns the most relevant documents

3. **IDE/ChatGPT Plugins**
   - Create plugins or custom GPTs that call the API to retrieve MO5 context
   - The retrieved content is used by the AI as additional context to answer questions

This enables any developer to benefit from a **shared, centralized knowledge base** accessible by AI tools.

---

## ⚙️ Implementation Steps

### Phase 1 — Knowledge Base Setup
- Collect and clean MO5-related documents (hardware docs, tutorials, sample code)
- Write a script to:
  - Chunk the documents into small pieces
  - Generate vector embeddings for each chunk (using OpenAI `text-embedding-3-small` or a local model)
  - Store them in a vector database (Qdrant, Weaviate, Milvus, or pgvector)

### Phase 2 — Knowledge API
- Build a REST API in C#/.NET with the following endpoints:
  - `POST /ask` — accepts a query and returns the top-N most relevant knowledge chunks
  - `POST /ingest` — (optional) to add or update documents
- The API should:
  - Compute the embedding of the incoming question
  - Perform a semantic similarity search against the database
  - Return the results as context text

### Phase 3 — Plugin Integration
- Create one or both of the following:
  - **Custom GPT** in ChatGPT with an OpenAPI schema pointing to your API
  - **Visual Studio / VS Code extension** that calls the API when the user asks MO5-related questions
- The plugin will forward the retrieved context along with the user’s code or question to the AI assistant (Copilot or GPT)

### Phase 4 — Documentation & Publishing
- Document the API with an `openapi.json` file
- Provide example queries and expected responses
- Optionally deploy the API publicly so other developers can use it

---

## 📚 References

- [OpenAI Docs: Retrieval-Augmented Generation (RAG)](https://platform.openai.com/docs/guides/rag)
- [OpenAPI Specification](https://spec.openapis.org/oas/latest.html)
- [LangChain .NET](https://github.com/hwchase17/langchain-dotnet)
- [Qdrant Vector Database](https://qdrant.tech/)
- [Milvus Vector Database](https://milvus.io/)

---

## ✅ Summary

| Step | Description |
|------|-------------|
| 1️⃣ Knowledge Base | Gather MO5 technical documents and code samples |
| 2️⃣ API | Build a REST API that returns relevant MO5 info from the knowledge base |
| 3️⃣ Plugins | Create IDE/ChatGPT plugins that query the API |
| 4️⃣ Integration | Assist developers with AI powered by your MO5 knowledge |

---

## 📌 Project Goals

- Centralize all knowledge about MO5 development
- Make it accessible via an API
- Allow AI assistants to leverage this API to help developers write C/Assembly code for the MO5
- Encourage community contributions to the knowledge base

---

> This repository will host the source code of the MO5 Knowledge API, its plugins, and all related documentation.
