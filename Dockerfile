FROM python:3.11-slim
RUN pip install discord.py
RUN pip install wordfreq
RUN pip install requests
COPY . .
ENTRYPOINT ["python", "src/main.py"]
