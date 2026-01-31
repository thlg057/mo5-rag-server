import os
from fastapi import FastAPI
from pydantic import BaseModel
from sentence_transformers import SentenceTransformer
import uvicorn

app = FastAPI()

# Récupération du modèle via l'environnement
MODEL_NAME = os.getenv("MODEL_NAME", "intfloat/multilingual-e5-small")

print(f"--- Chargement du modèle : {MODEL_NAME} ---")
model = SentenceTransformer(MODEL_NAME)

class EmbedRequest(BaseModel):
    texts: list[str]

@app.post("/embed")
async def embed(request: EmbedRequest):
    embeddings = model.encode(request.texts)
    return embeddings.tolist()

# AJOUTER CECI pour le Healthcheck Docker
@app.get("/health")
async def health():
    return {"status": "ok", "model": MODEL_NAME}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=5000)