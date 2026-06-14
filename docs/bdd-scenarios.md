# Behavior-Driven Development (BDD) Scenarios

Access to and control of estimation rooms in `es-timados` is governed by the following formal scenarios written in **Gherkin (Given-When-Then)** language. All of these scenarios are implemented and validated using end-to-end (E2E) integration tests within the test project.

### **Feature:** Access to estimation rooms in "es-timados"
**As** a team user (Moderator, Product Owner, or Developer)  
**I want** to identify myself and select my role when entering the website  
**So that** I can create a new room or join an existing one securely.

#### **Happy Paths**

*   **Scenario 1: A Moderator enters the website and creates a new room**
    *   **Given** that a user accesses the "es-timados" website
    *   **When** they enter their name as "Carlos"
    *   **And** they select the role "Moderator"
    *   **Then** the system creates a new room with a unique UUID
    *   **And** the user enters the room directly as Moderator.

*   **Scenario 2: A Developer or PO joins a room and is approved by the Moderator**
    *   **Given** that a user accesses the "es-timados" website
    *   **When** they enter their name as "Ana"
    *   **And** they select the role "Developer" (or "Product Owner")
    *   **And** they provide the UUID of an active room
    *   **And** the Moderator of that room approves the join request
    *   **Then** the user enters the waiting room.

#### **Edge Cases**

*   **Scenario 3: The user already has a saved name and decides to change it**
    *   **Given** that a user accesses the website and the system detects that they already have the name "Ana" saved in session
    *   **When** the user indicates they want to change their name
    *   **And** updates their name to "Ana Developer"
    *   **And** selects the role "Developer" and enters a valid UUID
    *   **Then** the join request sent to the Moderator must display the updated name "Ana Developer".

*   **Scenario 4: The Developer enters an invalid UUID or an inactive room UUID**
    *   **Given** that a user accesses the website and selects the role "Developer"
    *   **When** they provide a UUID code that does not exist or corresponds to a closed room
    *   **Then** the system must display an error message indicating that the room does not exist
    *   **And** the user must remain on the entry screen.

*   **Scenario 5: The Moderator rejects the join request**
    *   **Given** that a user with the role "Product Owner" has requested to join a room using a valid UUID
    *   **When** the Moderator inside the room rejects the join request
    *   **Then** the user receives an "Access denied" notification
    *   **And** the user does not enter the waiting room.

*   **Scenario 6: Attempting to proceed without completing mandatory fields**
    *   **Given** that a user accesses the "es-timados" website
    *   **When** they attempt to access a room leaving the name or role blank
    *   **Then** the system must prevent them from proceeding
    *   **And** must display validation messages indicating that the name and role are mandatory.

*   **Scenario 7: The Moderator disconnects before approving the user**
    *   **Given** that a "Developer" has requested to join a room with a valid UUID
    *   **And** is waiting for the Moderator's approval
    *   **When** the Moderator loses connection or closes the room before responding
    *   **Then** the "Developer" must receive a "Room closed or Moderator disconnected" message
    *   **And** their join request must be cancelled.
