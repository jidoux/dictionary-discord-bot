FROM python:3.14-slim
RUN pip install wordfreq
COPY . .
CMD ["timeout", "30s", "python", "word_list_creation.py"]
