# Context and Rules of the Game: es-timados

## 1. Introduction and Purpose of the System
In the agile development ecosystem, estimation serves to achieve technical alignment across the entire development team. `es-timados` is an estimation system designed to optimize cognitive load, reduce unnecessary discussions about scale magnitudes, and keep the product backlog in a highly granular and actionable state.

---

## 2. Estimation Sequence (Story Points)
The system uses a simplified numerical scale to represent relative effort:

$$\text{es-timados Scale: } \{1, 2, 3, 5, 8\}$$

### The "Limit of 8"
Any requirement or task that the development team considers to be greater than **8** points is assumed to have an unmanageable level of uncertainty or complexity for an efficient sprint. Instead of debating larger values (such as 13, 20, or 40), estimation for that item is halted, and the **Axe** card protocol is immediately activated.

---

## 3. Special Cards
The system features a deck of cards with specific purposes to streamline the workflow:

*   **Axe**: An immediate action card. It indicates that the User Story is too complex or ambiguous and must be split into smaller tasks or requirements before proceeding.
*   **Diagram**: Indicates that the technical team needs to sketch an architecture, a visual flow diagram, or hold a brief design discussion before assigning an estimation score.
*   **AI (Artificial Intelligence)**: Used to tag repetitive or mechanical tasks that are candidates to be automated or accelerated significantly using AI assistants and code generators.
*   **Coffee Cup**: Indicates that the team is experiencing mental fatigue and proposes taking a short break of a few minutes to restore focus.

---

## 4. Execution Protocol
Estimation sessions follow a structured five-step flow led by the Moderator:

1.  **Step 1: Story Presentation**: The Product Owner presents the requirements (acceptance criteria, details, etc.) while the team discusses them and clarifies technical queries.
2.  **Step 2: Private Estimation**: Each developer selects a card in secret (numbers 1 to 8, or a special card).
3.  **Step 3: Simultaneous Reveal**: All participants show their cards at the same time to prevent anchoring and authority bias.
4.  **Step 4: Consensus Management and Outliers**:
    *   *Consensus*: If all votes match, the value is recorded.
    *   *Discrepancy*: If there are differences, the extreme values (highest and lowest) explain their technical reasoning.
5.  **Step 5: Re-estimation**: After a brief discussion, the private vote is repeated until consensus is reached.

---

## 5. Roles and Responsibilities
*   **Product Owner**: Defines the scope, clarifies acceptance criteria, and prioritizes requirements. Acts as a functional consultant but has no vote in the estimation of effort.
*   **Development Team**: Determines the relative technical effort and complexity. Has the exclusive authority to vote and estimate tasks.
*   **Scrum Master / Moderator**: Facilitates the dynamic, keeps track of discussion times, and ensures the **Axe** protocol is executed when complex requirements arise.
