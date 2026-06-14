# es-timados - Sistema de Estimación Ágil

`es-timados` es una plataforma web y API para la estimación de esfuerzo en proyectos de desarrollo de software basada en una versión optimizada del Planning Poker tradicional. El sistema promueve la alineación técnica rápida, reduce la carga cognitiva y fomenta la granularidad del backlog.

---

## 1. Escenarios de Comportamiento (BDD)

El acceso y control de salas se rige por los siguientes escenarios formales escritos en lenguaje **Gherkin (Given-When-Then)**. Todos estos escenarios se encuentran implementados y validados mediante pruebas integrales de extremo a extremo (E2E).

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

---

## 2. Guía Comparativa de Reglas: es-timados vs. Planning Poker Tradicional

### 2.1. Introducción y Propósito del Sistema
En el desarrollo ágil, la estimación sirve para lograr la alineación técnica y mitigar sesgos como el anclaje. El sistema `es-timados` optimiza la carga cognitiva del equipo eliminando rangos innecesarios de incertidumbre para agilizar las sesiones de estimación.

### 2.2. Comparativa de la Secuencia de Estimación (Puntos de Historia)

| Planning Poker Tradicional | Sistema es-timados |
| :--- | :--- |
| `0, 1/2, 1, 2, 3, 5, 8, 13, 20, 40, 100` | `1, 2, 3, 5, 8` |

#### Explicación Técnica y el "Límite del 8"
La secuencia de Fibonacci refleja la incertidumbre creciente. Sin embargo, distinguir magnitudes grandes (ej. 39 vs 40) es imposible. En `es-timados`, limitamos la escala al valor máximo de **8**. Si el equipo considera que una tarea supera este límite, no se pierde tiempo debatiendo el número exacto: se asume que la complejidad es inmanejable para un sprint eficiente y se activa el protocolo de la carta **"Hacha"**.

### 2.3. Evolución de Cartas Especiales: De la Observación a la Acción

*   **Del Infinito (∞) al Hacha**: En el modelo tradicional, el infinito es un indicador pasivo de que la tarea es "muy grande". En `es-timados`, la carta **Hacha** es una instrucción activa: la Historia de Usuario debe dividirse (split) en sub-tareas procesables de inmediato.
*   **Del Signo de Interrogación (?) al Diagrama**: En lugar de simplemente admitir ignorancia, la carta **Diagrama** propone una acción inmediata: el equipo necesita una sesión de diseño visual, arquitectura o aclaración técnica antes de asignar puntos.
*   **Incorporación de la IA**: Una carta exclusiva de `es-timados`. Se utiliza para marcar tareas repetitivas o técnicas propensas a ser automatizadas o resueltas mediante asistentes de Inteligencia Artificial para acelerar los desarrollos.
*   **Taza de Café**: Se mantiene como el estándar universal para comunicar fatiga y proponer un descanso grupal.

---

## 3. Protocolo de Ejecución (Reglas del Juego)

Para garantizar la efectividad del consenso, las votaciones siguen estos pasos:

1.  **Paso 1: Presentación de la Historia**: El Product Owner presenta los requerimientos y Criterios de Aceptación.
2.  **Paso 2: Discusión Clarificadora**: El equipo debate riesgos y dudas técnicas. El PO aclara el "qué", no el "cuánto".
3.  **Paso 3: Estimación Privada**: Cada desarrollador selecciona secretamente su carta (1-8 o especiales).
4.  **Paso 4: Revelación Simultánea**: Todos muestran su carta a la vez para evitar el anclaje y el sesgo de autoridad.
5.  **Paso 5: Gestión de Consenso y Valores Atípicos**:
    *   *Consenso*: Si los votos coinciden, se registra el estimado oficial.
    *   *Atípicos*: Si hay discrepancias, los votos extremos explican su razonamiento.
6.  **Paso 6: Re-estimación**: Tras la discusión corta, se repite la votación privada hasta converger.

---

## 4. Roles y Responsabilidades

*   **Product Owner**: Define el alcance, aclara criterios de aceptación y prioridades. No vota ni estima el esfuerzo.
*   **Equipo de Desarrollo**: Determina el esfuerzo y la complejidad técnica. Tiene el poder exclusivo de votación.
*   **Scrum Master / Moderador**: Facilita la sesión, cuida los tiempos y vela por que el "Hacha" sea invocada ante historias que superen el límite del 8.

---

## 5. Resumen de Beneficios

*   **Predictibilidad de la Velocidad**: Un tamaño tope de 8 puntos estabiliza y hace predecible la velocidad del equipo.
*   **Reducción del Desperdicio**: Filtrar tareas ambiguas y grandes reduce el riesgo de tareas arrastradas entre sprints (*carry-over*).
*   **Backlog Lean**: El protocolo de división por el hacha mantiene un backlog ordenado, granular y procesable.
*   **Enfoque Estratégico**: Cartas como "Diagrama" e "IA" guían al equipo a idear soluciones prácticas en lugar de solo contar números.
