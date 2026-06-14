# Contexto y Reglas del Juego: es-timados

## 1. Introducción y Propósito del Sistema
En el ecosistema del desarrollo ágil, la estimación sirve para lograr la alineación técnica de todo el equipo de desarrollo. `es-timados` es un sistema de estimación diseñado para optimizar la carga cognitiva, reducir discusiones innecesarias sobre magnitudes de escala y mantener el backlog de producto en un estado altamente granular y procesable.

---

## 2. Secuencia de Estimación (Puntos de Historia)
El sistema utiliza una escala numérica simplificada para representar el esfuerzo relativo:

$$\text{Escala de es-timados: } \{1, 2, 3, 5, 8\}$$

### El "Límite del 8"
Cualquier requerimiento o tarea que el equipo de desarrollo considere superior a **8** puntos se asume que posee un nivel de incertidumbre o complejidad inmanejable para un sprint eficiente. En lugar de debatir sobre valores más grandes (como 13, 20 o 40), se detiene la estimación del ítem y se activa inmediatamente el protocolo de la carta **Hacha**.

---

## 3. Cartas Especiales
El sistema cuenta con un juego de cartas con propósitos específicos para agilizar el flujo de trabajo:

*   **Hacha**: Una carta de acción inmediata. Indica que la Historia de Usuario es demasiado compleja o ambigua y debe dividirse (*split*) en tareas o requerimientos más pequeños antes de continuar.
*   **Diagrama**: Indica que el equipo técnico necesita realizar un bosquejo de arquitectura, un diagrama de flujo visual o una breve discusión de diseño antes de poder asignar una puntuación de estimación.
*   **IA (Inteligencia Artificial)**: Se utiliza para marcar tareas repetitivas, mecánicas o propensas a resolverse o acelerarse notablemente mediante el uso de asistentes de IA y generadores de código.
*   **Taza de Café**: Indica que el equipo experimenta fatiga mental y se propone tomar un breve descanso de unos minutos para restaurar el enfoque.

---

## 4. Protocolo de Ejecución
Las sesiones de estimación siguen un flujo estructurado de seis pasos dirigidos por el Moderador:

1.  **Paso 1: Presentación de la Historia**: El Product Owner presenta los requerimientos y detalla los Criterios de Aceptación.
2.  **Paso 2: Discusión Clarificadora**: El equipo de desarrollo discute riesgos y resuelve dudas técnicas con el Product Owner (quien aclara el *qué* pero no el *cuánto*).
3.  **Paso 3: Estimación Privada**: Cada desarrollador selecciona una carta en secreto (números del 1 al 8, o una carta especial).
4.  **Paso 4: Revelación Simultánea**: Todos los participantes muestran su carta al mismo tiempo para evitar sesgos de anclaje y de autoridad.
5.  **Paso 5: Gestión de Consenso y Valores Atípicos**:
    *   *Consenso*: Si todos coinciden, se registra el valor.
    *   *Discrepancia*: Si hay diferencias, los valores extremos (más alto y más bajo) explican su razonamiento técnico.
6.  **Paso 6: Re-estimación**: Tras una discusión breve, se repite la votación privada hasta converger en un acuerdo.

---

## 5. Roles y Responsabilidades
*   **Product Owner**: Define el alcance, aclara los criterios de aceptación y prioriza los requerimientos. Actúa como consultor funcional, pero no tiene voto en la estimación de esfuerzo.
*   **Equipo de Desarrollo**: Determina el esfuerzo técnico relativo y la complejidad. Tiene la potestad exclusiva de votar y estimar las tareas.
*   **Scrum Master / Moderador**: Facilita la dinámica, cuida los tiempos de discusión y vela por que el protocolo del **Hacha** se ejecute ante requerimientos complejos.
