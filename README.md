# Week07 – Item Sorter Robot

## 1. Project Overview
This project extends the Week06 Inventory System by adding a robot arm simulation.  
The robot automatically picks ordered items from three inventory boxes (a, b, c) and places them into a shipment box (S) for delivery preparation.

The system communicates with a URScript-compatible robot simulator through TCP sockets.

---

## 2. Functionality
- Reads and processes the next available order from the order list.
- Moves the robot to pick items from boxes `a`, `b`, and `c` based on `InventoryLocation`.
- Places items in the shipment box `S`.
- Waits for 9.5 seconds between movements to allow time for robot execution.
- The shipment box is assumed to be replaced automatically after each order.

---

## 3. Coordinate System
Each grid unit represents **0.1 meters (10 cm)**.

| Box | X (m) | Y (m) | Description |
|------|-------|-------|-------------|
| a | 0.1 | 0.1 | Inventory Box A |
| b | 0.2 | 0.1 | Inventory Box B |
| c | 0.3 | 0.1 | Inventory Box C |
| S | 0.3 | 0.3 | Shipment Box |

The robot moves down 0.1 m (Z-axis) to pick or release an item.  
The gripper action is automatic and does not require additional programming.

---

## 4. Connection Settings

| Type | Port | Description |
|------|------|-------------|
| URScript | 30002 | Receives movement scripts |
| Dashboard | 29999 | Enables and releases robot brakes |
| IP Address | localhost | Change if simulator runs on another device |

---

## 5. How to Run
1. Start the **robot simulator** and ensure it listens on ports **29999** and **30002**.
2. Open the project in **Visual Studio** or **VS Code**.
3. Ensure the target framework is **.NET 6 or later**.
4. Run the program (`Program.cs`).
5. When prompted, press **Enter** to process the next order.
6. Observe console messages showing which item is being picked.
