import yfinance as yf
import pandas as pd
import matplotlib.pyplot as plt

# Define the ticker list
tickers_list = ['NVDA', 'TSLA']

# Fetch data
data = yf.download(tickers_list,'2021-01-01')['Adj Close']

# Calculate the daily percentage change
data_pct_change = data.pct_change()

# Calculate the growth of $1 invested in the stock
data_growth = (1 + data_pct_change).cumprod()

# Plot the data
data_growth.plot(figsize=(10, 7))
plt.title("Growth of $1 investment in stock since the 2021 year start", fontsize=16)
plt.ylabel('Value (in $)')

# Show the plot
plt.show()