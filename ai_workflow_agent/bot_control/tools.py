from langchain_core.tools import tool
from faiss_control.retrieve_context import get_context
@tool
def retrieve_context(question: str) -> str:
    """
    Consult all the documentation to check if the user question is present.

    Args:
        question (str): The question asked by the user.

    Returns:
        str: Text associated with the user's question.
    """
    try:
        all_context = get_context(question)

        if not all_context:
            return "Sorry, I do not have the information for you request"

        return all_context
    except Exception as e:
        print("Error in retrieve_context tool: ", str(e))
        return "Sorry, I had a problem retrieving the information for your request"