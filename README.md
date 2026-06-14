# es-timados - Sistema de Estimación Ágil

`es-timados` es una plataforma web y API para la estimación de esfuerzo en proyectos de desarrollo de software. Este sistema optimiza el proceso clásico de estimación reduciendo la carga cognitiva de los equipos mediante una escala de puntos delimitada y cartas de acción especializadas para evitar discusiones bizantinas y promover backlogs granulares.

---

## Documentación de Contexto y Reglas
El funcionamiento detallado del sistema, los roles y las cartas especiales se describen en la guía de [Reglas y Contexto del Juego](file:///home/wolfremium/RiderProjects/Wolfremium.Estimados/docs/es-timados-context.md).

---

## Escenarios de Comportamiento (BDD)

El acceso y control de salas de estimación se rige por los siguientes escenarios formales escritos en lenguaje **Gherkin (Given-When-Then)**. Todos estos escenarios se encuentran implementados y validados mediante pruebas integrales de extremo a extremo (E2E) dentro del proyecto de pruebas.

### **Feature:** Acceso a las salas de estimación en "es-timados"
**Como** usuario del equipo (Moderador, Product Owner o Developer)  
**Quiero** identificarme y seleccionar mi rol al entrar a la web  
**Para** poder crear una sala nueva o unirme a una existente de forma segura.

#### **Escenarios Principales (Happy Paths)**

*   **Escenario 1: Un Moderador entra a la web y crea una sala nueva**
    *   **Dado** que un usuario accede a la web de "es-timados"
    *   **Cuando** introduce su nombre como "Carlos"
    *   **Y** selecciona el rol de "Moderador"
    *   **Entonces** el sistema crea una nueva sala con un UUID único
    *   **Y** el usuario entra directamente a la sala como Moderador.

*   **Escenario 2: Un Developer o PO se une a una sala y es aprobado por el Moderador**
    *   **Dado** que un usuario accede a la web de "es-timados"
    *   **Cuando** introduce su nombre como "Ana"
    *   **Y** selecciona el rol de "Developer" (o "Product Owner")
    *   **Y** proporciona el UUID de una sala activa
    *   **Y** el Moderador de esa sala aprueba la solicitud de entrada
    *   **Entonces** el usuario ingresa a la sala de espera.

#### **Casos Extremos (Edge Cases)**

*   **Escenario 3: El usuario ya tiene un nombre guardado y decide cambiarlo**
    *   **Dado** que un usuario accede a la web y el sistema detecta que ya tiene el nombre "Ana" guardado en sesión
    *   **Cuando** el usuario indica que desea cambiar su nombre
    *   **Y** actualiza su nombre a "Ana Developer"
    *   **Y** selecciona el rol de "Developer" e introduce un UUID válido
    *   **Entonces** la solicitud de entrada enviada al Moderador debe mostrar el nombre actualizado "Ana Developer".

*   **Escenario 4: El Developer introduce un UUID inválido o de una sala inactiva**
    *   **Dado** que un usuario accede a la web y selecciona el rol de "Developer"
    *   **Cuando** proporciona un código UUID que no existe o corresponde a una sala cerrada
    *   **Entonces** el sistema debe mostrar un mensaje de error indicando que la sala no existe
    *   **Y** el usuario debe permanecer en la pantalla de ingreso.

*   **Escenario 5: El Moderador rechaza la solicitud de acceso**
    *   **Dado** que un usuario con el rol de "Product Owner" ha solicitado entrar a una sala mediante un UUID válido
    *   **Cuando** el Moderador desde dentro de la sala rechaza la solicitud de entrada
    *   **Entonces** el usuario recibe una notificación de "Acceso denegado"
    *   **Y** el usuario no ingresa a la sala de espera.

*   **Escenario 6: Intentar avanzar sin completar los campos obligatorios**
    *   **Dado** que un usuario accede a la web de "es-timados"
    *   **Cuando** intenta acceder a una sala dejando el nombre o el rol en blanco
    *   **Entonces** el sistema debe impedir el avance
    *   **Y** debe mostrar mensajes de validación indicando que el nombre y el rol son obligatorios.

*   **Escenario 7: El Moderador se desconecta antes de aprobar al usuario**
    *   **Dado** que un "Developer" ha solicitado entrar a una sala con un UUID válido
    *   **Y** está esperando la aprobación del Moderador
    *   **Cuando** el Moderador pierde la conexión o cierra la sala antes de responder
    *   **Entonces** el "Developer" debe recibir un mensaje de "Sala finalizada o Moderador desconectado"
    *   **Y** su solicitud de acceso debe ser cancelada.
