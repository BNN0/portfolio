import os
from dotenv import load_dotenv
from pathlib import Path
from langchain_community.vectorstores import FAISS
from aws_config.aws import bedrock_embeddings

load_dotenv()
# Define the path for temporary storage
folder_path = os.getenv("FAISS_FOLDER_PATH")

os.makedirs(folder_path, exist_ok=True)

def get_context_faiss(question, faiss_files=None):
    try:
        prompt = "{context}"

        if faiss_files is None:
            faiss_files = list(Path(folder_path).glob("*.faiss"))
        else:
            if isinstance(faiss_files, str):
                faiss_files = [Path(folder_path) / f"{faiss_files}.faiss"]
            elif isinstance(faiss_files, list):
                faiss_files = [Path(folder_path) / f"{file}.faiss" for file in faiss_files]

        all_retrieved_docs = []

        for faiss_file in faiss_files:
            if not faiss_file.exists():
                continue

            faiss_index = FAISS.load_local(
                index_name=faiss_file.stem,
                folder_path=folder_path,
                embeddings=bedrock_embeddings(),
                allow_dangerous_deserialization=True
            )
            retrieved_docs = faiss_index.as_retriever(
                search_type="similarity",
                search_kwargs={"k": 4}
            ).invoke(question)
            all_retrieved_docs.extend(retrieved_docs)

        context = "\n".join([doc.page_content for doc in all_retrieved_docs])
        complete_prompt = prompt.format(context=context, question=question)
        return complete_prompt
    
    except Exception as e:
        print("Error in get_context_faiss: ", str(e))
        return "Error retrieving context"


def get_context(question):
    try:
        print("RETRIEVING ALL CONTEXT...")
        complete_prompt = get_context_faiss(question)
        return complete_prompt
    except Exception as e:
        print("Error in get_context", str(e))
        return "Error retrieving context"
