# Behavior-Driven Development (BDD) Scenarios

Access to, control of, and estimation within rooms in `es-timados` are governed by the following formal scenarios written in **Gherkin (Given-When-Then)** language.

---

## Feature: Access to estimation rooms in "es-timados"
**As** a team user (Moderator, Product Owner, or Developer)  
**I want** to identify myself and select my role when entering the website  
**So that** I can create a new room or join an existing one securely.

### Happy Paths

*   **Scenario 1: A Moderator enters the website and creates a new room [Implemented]**
    *   **Given** that a user accesses the "es-timados" website
    *   **When** they enter their name as "Carlos"
    *   **And** they select the role "Moderator"
    *   **Then** the system creates a new room with a unique UUID
    *   **And** the user enters the room directly as Moderator.

*   **Scenario 2: A Developer or PO joins a room and is approved by the Moderator [Implemented]**
    *   **Given** that a user accesses the "es-timados" website
    *   **When** they enter their name as "Ana"
    *   **And** they select the role "Developer" (or "Product Owner")
    *   **And** they provide the UUID of an active room
    *   **And** the Moderator of that room approves the join request
    *   **Then** the user enters the waiting room.

### Edge Cases

*   **Scenario 3: The user already has a saved name and decides to change it [Implemented]**
    *   **Given** that a user accesses the website and the system detects that they already have the name "Ana" saved in session
    *   **When** the user indicates they want to change their name
    *   **And** updates their name to "Ana Developer"
    *   **And** selects the role "Developer" and enters a valid UUID
    *   **Then** the join request sent to the Moderator must display the updated name "Ana Developer".

*   **Scenario 4: The Developer enters an invalid UUID or an inactive room UUID [Implemented]**
    *   **Given** that a user accesses the website and selects the role "Developer"
    *   **When** they provide a UUID code that does not exist or corresponds to a closed room
    *   **Then** the system must display an error message indicating that the room does not exist
    *   **And** the user must remain on the entry screen.

*   **Scenario 5: The Moderator rejects the join request [Implemented]**
    *   **Given** that a user with the role "Product Owner" has requested to join a room using a valid UUID
    *   **When** the Moderator inside the room rejects the join request
    *   **Then** the user receives an "Access denied" notification
    *   **And** the user does not enter the waiting room.

*   **Scenario 6: Attempting to proceed without completing mandatory fields [Implemented]**
    *   **Given** that a user accesses the "es-timados" website
    *   **When** they attempt to access a room leaving the name or role blank
    *   **Then** the system must prevent them from proceeding
    *   **And** must display validation messages indicating that the name and role are mandatory.

*   **Scenario 7: The Moderator disconnects before approving the user [Implemented]**
    *   **Given** that a "Developer" has requested to join a room with a valid UUID
    *   **And** is waiting for the Moderator's approval
    *   **When** the Moderator loses connection or closes the room before responding
    *   **Then** the "Developer" must receive a "Room closed or Moderator disconnected" message
    *   **And** their join request must be cancelled.

---

## Feature: Estimation Session and Voting Protocol
**As** a development team user (Moderator, Product Owner, or Developer)  
**I want** to estimate user stories using the es-timados deck and protocol  
**So that** we can estimate tasks efficiently and identify high-complexity requirements early.

### Happy Paths

*   **Scenario 8: A Developer casts a valid story point vote [Implemented]**
    *   **Given** that the room is in the private estimation stage
    *   **When** a Developer votes with a valid card "5"
    *   **Then** the vote is registered secretly
    *   **And** the vote is not visible to other participants.

*   **Scenario 9: Simultaneous reveal and consensus check [Implemented]**
    *   **Given** that the room is in the private estimation stage
    *   **And** all developers have cast their votes as "5"
    *   **When** the Moderator triggers the reveal
    *   **Then** the system reveals all votes simultaneously
    *   **And** the estimation session registers a consensus of "5" points.

*   **Scenario 10: Simultaneous reveal with discrepancies requires re-estimation [Implemented]**
    *   **Given** that the room is in the private estimation stage
    *   **And** Developer "Carlos" voted "3" and Developer "Ana" voted "5"
    *   **When** the Moderator triggers the reveal
    *   **Then** the system reveals all votes
    *   **And** the estimation session detects a discrepancy
    *   **And** the system prompts for discussion and re-estimation.

### Edge Cases

*   **Scenario 11: A Product Owner attempts to vote [Implemented]**
    *   **Given** that the room is in the private estimation stage
    *   **When** a Product Owner attempts to vote
    *   **Then** the system prevents the Product Owner from voting
    *   **And** displays an error indicating that Product Owners cannot vote.

*   **Scenario 12: A Developer votes with the Hacha (Axe) card [Implemented]**
    *   **Given** that the room is in the private estimation stage
    *   **When** any Developer votes with the "Hacha" card
    *   **And** the Moderator reveals the cards
    *   **Then** the estimation session is halted
    *   **And** the system notifies the team that the story is too complex and must be split.

*   **Scenario 13: Developer votes with other special cards (Diagrama, IA, Taza de Café) [Implemented]**
    *   **Given** that the room is in the private estimation stage
    *   **When** a Developer votes with "Diagrama" (or "IA" or "Taza de Café")
    *   **And** the Moderator reveals the cards
    *   **Then** the system flags the special card and recommends the corresponding action (technical discussion, AI automation tagging, or taking a break).

