# What is this?
This is a multi-agent system designed to simulate microgrid energy trading auctions. It can be used for simulations, evaluations of trading/auction strategies, testing of different architecture patterns, finding optimal solution for communcation between agents and etc. Each agent is a microgrid member(agent/household) with a few parameters:
* Energy Consumption (in kWhs)
* Energy Generation (in kWhs)
* Price to buy 1kWhs from Utility Company
* Price to sell 1kWhs to Utility Company
Each agent has to determine if he is a "seller" or a "buyer" based on his consumption/generation. Each agent also has to come up with a price for buying or selling, based. This can be achived by multiple approaches. The approach used is described in detail below. 
The system consists of two auction protocols - Dutch Auction Protocol and Double Auction Protocol. 

# Auction 1. – Dutch Auction
Fully decentralized. Designed to compare against a superior “Double Acution” with a central agent and showcase the inability to scale, at least in the current implementa-tion, because of its significant number of messages exchanged between agets. The price changes and justification of the choice of auction is explained in the “dutchAuction-Offer“.
The Dutch auctiom model in as an auction in microgrid p2p systems performs the worse than the rest. This partly motivates the choice, simply to showcase how ineffi-cient the Dutch auction is to Double Auction. (Teixeira et al., 2021) One of the few benefits the Dutch auction has is due to its open nature it is not too hard to develop in a fully decentralized p2p system with buyers and sellers. (Fontoura et al., 2005)
Messages that agents exchange
* “inform“ – received from Environment Agent by everyone, activates “HandleInformation” function which decodes the message and broadcast to all agents it’s Status and Energy balance. As fully decentralized system, in order for each agent to know who is selling and who is buying, each agent has to send and receive the others status (buying, selling) and energy balance (+n, -n).
* “broadcast” – not the best message name, received by everyone, activates “HandleBoradcast” function which calculates the overall energy in the sys-tem and adds each message recived to a “messages” list. Waits for 2 turns and activates “All messages recived function” Then Dutch_Auction is an-nounced, and each seller send to the buyer its initial price. The initial price of the dutch auction for each seller is 22 – the max price that the buyer can buy energy from the Utility Company.
* “dutchAuctionOffer” – revied by each buyer, activates a function “Han-dleDuchAuctionOffer” which calculates if the offer is accepted or not. Buyer accepts the offer if the price is less than its price to buy from Utility Company. The idea behind calculating the price like that, was the Dutch auction was meant to be activated only when the overall energy of the sys-tem was negative (e.g. there is energy deficit, therefore there are more buy-ers). This way for calculating the price is beneficial for the sellers, as they sell their energy on the highest price possible. If there is no answer from the buyers the seller lowers it’s price with 1. If the buyer’s energy is 0 he is no longer interested in the auction and he sends “buyerOut” message to the sender.
* “buyerOut” – remove a buyer from the buyers list
* “sellerOut” – remove a seller from the sellers list
* “dutchAuctionOfferAccept” – recived by the seller if the buyer is happy with the price. In this scenario the seller sends the the full demand from the buyer or a partial amount, depending on the seller energy availability. If his
3
energy after the send is 0, he refuses each offer sent to him. Seller sends “sellerOut” if he executes a trade and his energy is 0.
* “refuseOffer” – recived by the buyer if he accepted a price from seller, but the seller no longer has the energy. On refuse offer the buyer removes the seller from the sellers list. If there are no sellers left in the list the auction is over and the buyer broadcast “noSellersLeft”
* “sendEnergy” – send by a seller to buyer. Contains the energy that is sent for the price its agreed on.
* “noSellersLeft’ – recived by anyone, means that the aucton is finished be-cause the is no more energy to be sold. All the buyers left with negative energy balance buy from the Utility Company.
* “noBuyersLeft” – recived by anyone. Same function as “noSellersLeft”. All the sellers left with positive energy balance sell to the Utility Company.

# Auction 2. – Double Auction
With central agent. Designed for maximum efficiency, much more scalable than the decentralized Dutch auction, because of the central agent, which acts as a “database” of sell bids and buy bids and also sends messages to each seller when his bid is met.
The sellers and buyers decide on how much they bid based on the overall energy of the system. If the energy in the system is positive then – there is more to sell and less demand, therefore the prices are lower and vice versa.
Double auction has been thoroughly studied and its proven to achieve good profit for a short-term or long-term. It is also efficient and suitable for a system which involves high amount of buyers and sellers. (Zhang et al., 2021)
### How the initial prices are calculated?
* Buyers
Midprice – (price to buy from utility company) – (price to buy from utility company) / 3 Price = Midprice + (Midprice * ( Overallenergy / 100 ));
In other words the midprice is increased or decreased in % depending on the energy balance of the system. If the system energy is 20 the buyers price would be 80% of the midprice. If the system energy is -20 the initial price for buyer would be 120% of the midprice. The midprice depends on the buyers “price to buy from the utility company”. In the sourcecode there is a second more advanced formula developed, but commented, as its to volatile for the current system energy amplitude.
* Sellers
For seller is slightly simpler with
Midprice = (price to sell to utility company) + 10;
Price = Midprice + (Midprice * ( Overallenergy / 100 ));

As with buyers the price is influenced by the demand – in this case the price to sell is higher when there is less energy in the system and vice versa.
### How prices are changed when there is no matches?
Depending on the overall energy in the system the prices are changed using a coefficient 0.4 for the agents in advantage (sellers when there is less energy or buyers when there is more energy) and 0.6 for agents in disadvantage. This is a decimal value which is kept for each agent and then converted to an int. 0.6 to int is 1 and 0.4 to int is 0 in-crease/decrease. Next round its 1.2 (0.6+0.6) and 0.8 (0.4 + 0.4). The idea is to incre-ment firstly the one in disadvantage, then the one in advantage. The more time there is no match the more advantage the agents with advantage have.
### Messages that the agents exchange
- “calculatePriceBuy” – send to each buyer from central, contains overall energy of the system. Message Info used by the seller to calculate its price for selling
- “calculatePriceSell” – send to each seller from central, contains overall energy of the system. Message Info used by the seller to calculate its price for selling
- “requestSendEnergy” – send to a seller from central when his sell price is matched by a buyer. Contains all the buyers that matched the price. The message contains only the amount that has to be send. -
- “centralInform” – send by the household to the central. Message contains the household information – name, status, energybalance
- “priceBuy” – send by households that are buying to the central
- “priceSell” – send by households that are selling to the central
- “sendEnergy” – send by a seller to buyer. Contains the energy that is sent for the price its agreed on.
- “noMatchBuyer” and “noMatchSeller” – recived by a household buyer/seller when ther is no match for their price. If the overall energy of the system is positive then the buyers have the advantage and their offer adjustment is smaller and vice versa
- “noBuyersLeft” and “noSellersLeft” - recived by sellers/buyers, means that the aucton is finished because the is no more energy to be traded. All the buyers left with negative energy balance buy from the Utility Company and vice versa.
Valid in both auctions:
“noSellersLeft” and “noBuyersLeft” terminate the auction and generate – [Agent Re-port], [Messages Report] and [Report Summary]

# Reports and Experiments
### [Report Messages]
This report is used to count the total amount of messages exchanged by the agents in the system. This does not calculate the messages after the end of the auction which are exchanged for the [Report Summary]. The messages exchanged for report summary are equal to the total amount of Household agents in the system.
Each message from each agent is appened to a file which lines are then counted. The result is all the messages exchanged in the system, combined.
![image](https://user-images.githubusercontent.com/61486268/205705658-ad1dbb33-178c-4c60-a2e5-7207b056b7c4.png)

### [Report Agent] and [Report Summary]
These reports is executed when the auction is over and after the messages report is finished.
Report summary is a summary of the the performance of the system. It includes the average buyer money, average seller money, overall energy and unclean energy bought.
![image](https://user-images.githubusercontent.com/61486268/205705822-7bd1d1a5-eff7-4b88-83a2-96b35e88e931.png)

It saves the output in a file in a csv format which can be used to generate graphs reports or databases.
It starts from Household 01 and finished with “agentNumber” which is supplied in the household. Can be used to generate report from agents from 1 to agentNumber. In this case its used to generate report for all the agents. It works as Household 01 sends its information to Household 02. This repeats in sequence until the last agent. For each agend its generating an [Agent Report] which includes essential agent info as role, and money balance:
![image](https://user-images.githubusercontent.com/61486268/205705836-e36f1dc9-634d-467e-bb9f-7a245a9e73d3.png)

### Performance and efficiency
The performance of the Double Auction is much better in terms of efficiency simply because there is a Central agent which cuts the messages significantly.
Average messages for 20 Agents in Dutch auction (fully decentralized) – 1235
Average messages for 20 Agents in Double auction (central agent) – 174
This displays the huge difference in the efficienty of the protocols and approach.

#### Important
To toggle between dutch auction and double auction change is isDutchAuction bool from seetings.cs file.

## Conclusion
In real world scenario the system being fully decentralized e.g. lacking a central agent would not be a huge benefit in the most cases. Overall the better computational effi-ciency would arguably more beneficial than not relying on central agent. This becomes apperant when the system scales and the time complexity really demonstrate the poor communication efficiency between the agent.

## References:
AhmadiAhangar, R., Rosin, A., Niaki, A. N., Palu, I., & Korõtko, T. (2019). A review on real-time simulation and analysis methods of microgrids. International Trans-actions on Electrical Energy Systems, 29(11), e12106. https://doi.org/10.1002/2050-7038.12106
Fontoura, M., Ionescu, M., & Minsky, N. (2005). Decentralized Peer-to-Peer Auctions. Electronic Commerce Research 2005 5:1, 5(1), 7–24. https://doi.org/10.1023/B:ELEC.0000045971.43390.C0
Teixeira, D., Gomes, L., & Vale, Z. (2021). Single-unit and multi-unit auction frame-work for peer-to-peer transactions. International Journal of Electrical Power & Energy Systems, 133, 107235. https://doi.org/10.1016/J.IJEPES.2021.107235
Zhang, C., Yang, T., & Wang, Y. (2021). Peer-to-Peer energy trading in a microgrid based on iterative double auction and blockchain. Sustainable Energy, Grids and Networks, 27, 100524. https://doi.org/10.1016/J.SEGAN.2021.100524
