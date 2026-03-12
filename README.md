 Elevate

**Elevate** is a simple elevator simulator built with **.NET 10**.

## Features

- Simulates **4 elevators**
- Operates in a **10-floor building**
- Automatically generates elevator requests
- Displays elevator movement through application logs
- Sends real-time updates via **SignalR**

## Testing

To test the application:

1. Start the **API**.
2. Send a request to the following endpoint:
    - [POST]'https://localhost:7097/api/elevator/loop'
3. Observe the **logs** to see
    - Elevator movement
    - Automatically generated requests
4. (Optional) Start the simple *React client application* that listens to **SignalR notifications** to visualize the elevator updates.
    - 'https://github.com/bojangavreski/ElevateClient'
## Note

This application is not production ready and there is a room for improvement in many aspects of it. However it is a great programming exercise. 

Enjoy :)

P.S I wanted to experiment with Microsoft Orleans, hence I made [ElevateV2](https://github.com/bojangavreski/ElevateV2)
