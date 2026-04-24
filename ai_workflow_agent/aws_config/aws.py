import boto3
import os
from langchain_aws.chat_models import ChatBedrock
from langchain_aws import BedrockEmbeddings

from dotenv import load_dotenv
load_dotenv()

folder_path = os.getenv("FAISS_FOLDER_PATH")
os.makedirs(folder_path, exist_ok=True)

# bedrock
def bedrock_client():
    client = boto3.client(
        service_name="bedrock-runtime",
        aws_access_key_id=os.getenv('AWS_ACCESS_KEY_ID'),
        aws_secret_access_key=os.getenv('AWS_SECRET_ACCESS_KEY'),
        region_name=os.getenv('REGION_BEDROCK', 'us-west-2')
    )
    return client

def bedrock_embeddings():
    bedrock_embeddings = BedrockEmbeddings(
        model_id="amazon.titan-embed-text-v1", 
        client=bedrock_client()
    )
    return bedrock_embeddings

# s3Bucket
def initialize_s3bucket():
    client = boto3.client(
        's3',
        aws_access_key_id=os.getenv('AWS_ACCESS_KEY_ID'),
        aws_secret_access_key=os.getenv('AWS_SECRET_ACCESS_KEY'),
        region_name=os.getenv('REGION_S3', 'us-west-2')
    )
    return client

# claude
def llm_client():
    client = boto3.client(
        'bedrock-runtime',
        aws_access_key_id=os.getenv('AWS_ACCESS_KEY_ID'),
        aws_secret_access_key=os.getenv('AWS_SECRET_ACCESS_KEY'),
        region_name=os.getenv('REGION_CLAUDE', 'us-west-2')
    )
    return client

def bedrock_claude(tools=None):
    model = ChatBedrock(
        model_id="us.anthropic.claude-3-5-sonnet-20241022-v2:0",
        model_kwargs={
            'temperature': 0.1,
            "max_tokens": 1000
        },
        client=llm_client()
    )
    
    if tools:
        model = model.bind_tools(tools)
    
    return model