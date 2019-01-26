# BTCMarketsForAmibroker
Example of pushing real-time data from BTCMarkets to Amibroker charting/trading software.

This .Net solution subscribes to the BTCMarkets API (websocket based), to receive realtime crypto-currency pricing data. This data is then pushed to a datasource plugin, hosted in Amibroker. From there it's a matter of setting up a realtime database, and using Amibroker charting to visualize activity.

Note this project does not have order placement, that's in another project I'll publish on GitHub soon.

## Requirements

* Professional edition of AmiBroker (Standard edition does not offer 1/5/15 second charts, and is 32-bit only)

* .Net for Amibroker - An SDK that allows .Net support for Amibroker, instead of writing C/C++ (Worth getting the Developer Edition compared to Standard).

* The BTC Markets public API is free, however if you plan on using AmiBroker to place orders, then of course you'll need a BTCMarkets account.


## References

[BTCMarkets API](https://github.com/BTCMarkets/API/wiki/websocket)

[Amibroker Charting software](http://amibroker.com)

[.NET for AmiBroker](http://www.dotnetforab.com)