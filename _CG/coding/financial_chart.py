import yfinance as yf
import pandas as pd
import matplotlib.pyplot as plt

def plot_stock_price_change():
    # Define the ticker list
    tickers_list = ['NVDA', 'TSLA']

    # Fetch data
    data = yf.download(tickers_list,'2021-01-01')['Adj Close']

    # Print first 5 rows of the data
    print(data.head())

    # Calculate the percentage change
    data_pct_change = data.pct_change()

    # Plot all the close prices
    ((data_pct_change+1).cumprod()-1).plot(figsize=(10, 7))

    # Show the plot
    plt.show()

plot_stock_price_change()