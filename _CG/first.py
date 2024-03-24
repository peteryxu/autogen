from autogen import AssistantAgent, UserProxyAgent
from autogen.coding import LocalCommandLineCodeExecutor

import os
from pathlib import Path

llm_config = {
    "config_list": [{"model": "gpt-4", "api_key": os.environ["OPENAI_API_KEY"]}],
}

work_dir = Path("coding")
work_dir.mkdir(exist_ok=True)

assistant = AssistantAgent("assistant", llm_config=llm_config)

code_executor = LocalCommandLineCodeExecutor(work_dir=work_dir)
user_proxy = UserProxyAgent(
    "user_proxy", code_execution_config={"executor": code_executor}
)

# Start the chat
user_proxy.initiate_chat(
    assistant,
    message="Plot a chart of NVDA and TESLA stock price change YTD.",
)