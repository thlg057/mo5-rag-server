FROM python:3.10-slim

WORKDIR /app

# 1. Installation des dépendances standards (PyPI)
RUN pip install --no-cache-dir fastapi uvicorn sentence-transformers

# 2. Installation de Torch version CPU uniquement (Index PyTorch)
RUN pip install --no-cache-dir torch --index-url https://download.pytorch.org/whl/cpu

COPY app.py .

EXPOSE 5000

CMD ["python", "app.py"]