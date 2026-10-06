# Flappy Bird AI — Multi-Agent Deep Reinforcement Learning

A **Deep Reinforcement Learning** project that trains autonomous agents to play Flappy Bird using a custom **Unity/C# environment** and a **PyTorch Dueling Double Deep Q-Network (D3QN)** implementation.

The project separates simulation from learning: Unity runs the game environment while Python handles neural-network inference, experience replay, and optimization. Multiple agents can interact with the environment to generate training experience.

## Overview

Flappy Bird provides a simple action space but a surprisingly useful reinforcement-learning problem.

At every timestep, an agent has to decide whether to:

```text
FLAP
```

or

```text
DO NOTHING
```

based on its current position and the surrounding obstacles.

A successful agent must learn the relationship between:

- the bird's position
- its vertical motion
- the position of upcoming pipes
- the pipe opening
- the consequences of flapping at different times

The objective is simple:

> **Survive for as long as possible while passing through as many pipes as possible.**

Instead of using an existing Gym environment, I implemented the game environment in **Unity** and connected it to a separately implemented reinforcement-learning system in **Python**.

---

# Architecture

```text
                    ┌────────────────────────────┐
                    │        Unity / C#          │
                    │                            │
                    │   Flappy Bird Environment  │
                    │   • Bird physics           │
                    │   • Pipe generation        │
                    │   • Collision detection    │
                    │   • Episode reset          │
                    │   • Reward/state output    │
                    └─────────────┬──────────────┘
                                  │
                           Socket Interface
                                  │
                    ┌─────────────▼──────────────┐
                    │      Python / PyTorch      │
                    │                            │
                    │        D3QN Agent          │
                    │   • Dueling network        │
                    │   • Double DQN             │
                    │   • Replay memory          │
                    │   • Target network         │
                    │   • ε-greedy exploration   │
                    └────────────────────────────┘
```

The environment and learning system communicate over a **socket connection**, allowing the Unity simulation and Python training process to run independently.

A typical interaction cycle looks like:

```text
Environment State
       ↓
Neural Network
       ↓
Choose Action
       ↓
Unity Executes Action
       ↓
Reward + Next State
       ↓
Store Experience
       ↓
Train D3QN
       ↓
Repeat
```

---

# Reinforcement Learning

## Deep Q-Learning

The agent learns an approximation of the action-value function:

```text
Q(state, action)
```

which estimates how valuable taking a particular action is from the current state.

For Flappy Bird, the agent compares the predicted value of:

```text
Q(state, FLAP)
Q(state, WAIT)
```

and chooses the action expected to produce the highest long-term reward.

During training, exploration is introduced using an **epsilon-greedy policy**, allowing the agent to discover new strategies instead of immediately committing to its current predictions.

---

## Dueling DQN

The neural network uses a **dueling architecture**, separating its estimate of:

```text
V(s)   — value of the current state
A(s,a) — advantage of each available action
```

before combining them into Q-values.

Conceptually:

```text
Q(s, a) = V(s) + A(s, a)
```

This allows the model to learn how favorable a state is independently from determining whether flapping is better than waiting.

That distinction is useful in Flappy Bird because many nearby states may have similar overall value even when the correct action changes.

---

## Double DQN

Traditional DQN can overestimate action values because the same network both selects and evaluates the best next action.

This project uses **Double DQN**, which separates those responsibilities between:

- the online network
- the target network

The online network selects the action while the target network evaluates it.

This reduces Q-value overestimation and generally improves training stability.

---

## Target Network

A separate target neural network is maintained alongside the actively trained model.

Rather than changing the training target after every gradient update, the target network is periodically synchronized with the primary network.

This provides a more stable learning objective.

---

# Multi-Agent Training

The environment was designed to support **multiple agents**, allowing several birds to generate experience during training rather than relying on a single sequential agent.

Conceptually:

```text
                   ┌── Agent 1
                   │
Unity Environment ─┼── Agent 2
                   │
                   ├── Agent 3
                   │
                   └── Agent N
                         │
                         ▼
                   Experience Data
                         │
                         ▼
                     D3QN Training
```

Parallelizing environment interaction increases the amount and diversity of experience that can be collected during training.

Different agents can encounter different pipe configurations and trajectories, creating a broader set of transitions for the model to learn from.

---

# Custom Unity Environment

The Flappy Bird simulation was implemented in **Unity/C#** rather than using an existing reinforcement-learning benchmark.

The environment handles:

- bird movement and gravity
- flap actions
- procedural pipe generation
- scrolling obstacles
- collisions
- scoring
- terminal states
- episode resets
- state extraction
- reward generation
- communication with the Python agent

Building the environment from scratch provides direct control over simulation behavior and makes it possible to modify training speed, observations, rewards, and episode mechanics.

---

# State → Action → Reward

The RL system can be viewed as three major components.

### State

The environment provides numerical information describing the current game situation.

The agent uses this information to infer whether flapping or waiting is more valuable.

### Action

The action space is intentionally small:

```text
0 → Do nothing
1 → Flap
```

Despite the binary action space, timing is critical. A flap that would be correct several frames later may immediately cause a collision if executed too early.

### Reward

Rewards communicate the objective of the game to the agent.

The training objective encourages the agent to:

- remain alive
- progress through the level
- successfully pass pipes

while heavily penalizing terminal collisions.

Over repeated episodes, Q-learning allows the agent to associate actions with their long-term consequences.

---

# Why Flappy Bird?

The game is visually simple, but it contains several useful reinforcement-learning challenges:

### Continuous decision making

The agent must repeatedly evaluate the environment rather than make a single prediction.

### Delayed consequences

An incorrect flap may not kill the bird immediately.

The consequence of an action can occur several timesteps later.

### Exploration vs. exploitation

The agent initially has no understanding of the game and must discover useful behavior through exploration.

### Dynamic observations

Pipe locations constantly change relative to the player, requiring the agent to generalize rather than memorize one fixed sequence.

### Timing-sensitive control

Correct behavior depends on both **what action is selected** and **when it is selected**.

---

# Tech Stack

| Component | Technology |
|---|---|
| Reinforcement Learning | Dueling Double DQN |
| Deep Learning | PyTorch |
| Training | Python |
| Simulation | Unity |
| Environment Logic | C# |
| Numerical Operations | NumPy |
| Communication | Python sockets |
| Training Strategy | Multi-agent environment interaction |

---

# Repository Structure

```text
Flappy-Bird-AI/
│
├── Flappy Bird AI/
│   └── Python reinforcement-learning implementation
│
├── Flappy Bird Environment/
│   └── Unity Flappy Bird environment
│
└── README.md
```

---

# Running the Project

## Requirements

The environment was developed using:

```text
Unity 2019.4
Python
PyTorch
NumPy
```

Python's built-in `socket` module is also used for communication between the training process and Unity.

## Setup

Clone the repository:

```bash
git clone https://github.com/MycoalDough/Flappy-Bird-AI.git
cd Flappy-Bird-AI
```

Install the Python dependencies required by the agent.

Then open the **Flappy Bird Environment** project using Unity 2019.4 and run the Python training process alongside it.

The Python RL process communicates with the Unity environment through sockets.

---

# Key Challenges

## Connecting Unity and PyTorch

The game simulation and reinforcement-learning model are implemented in completely different runtimes.

I therefore needed to create a communication layer capable of transferring:

```text
State → Python
Action → Unity
Reward → Python
Terminal State → Python
```

while keeping the environment and learning process synchronized.

---

## Training Stability

Deep Q-learning can become unstable because the network is learning from predictions generated by another changing neural network.

The combination of:

- Double DQN
- a target network
- replay memory
- controlled exploration

helps reduce instability during training.

---

## Reward Design

A reinforcement-learning agent only optimizes the objective encoded by its rewards.

Designing useful rewards therefore required balancing immediate survival against the actual objective of passing obstacles and remaining alive over longer trajectories.

Poor reward design can easily produce behavior that maximizes numerical reward without actually playing the game correctly.

---

## Efficient Experience Collection

Reinforcement learning can require large amounts of environment interaction.

Supporting multiple agents allows the system to generate more varied gameplay experience and expose the network to a wider range of states during training.

---

# What I Learned

This project was one of my early experiments with building a complete deep reinforcement-learning pipeline rather than only training a model on an existing dataset.

It gave me hands-on experience with:

- implementing Deep Q-Learning in PyTorch
- Dueling DQN architectures
- Double DQN
- target networks
- experience replay
- epsilon-greedy exploration
- reward design
- state-space design
- multi-agent simulation
- Unity/Python communication
- designing custom RL environments
- debugging interactions between a model and a simulator

Most importantly, the project showed me that reinforcement learning is not only a neural-network problem.

The quality of the **environment, observations, rewards, exploration strategy, and training infrastructure** can have just as much impact on the final behavior as the architecture of the model itself.

---

## Disclaimer

This project is an educational recreation of the gameplay mechanics of *Flappy Bird* for reinforcement-learning experimentation.

*Flappy Bird* and its original assets/concepts belong to their respective rights holders.
