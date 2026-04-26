-- ============================================================
--  Essence MVP — Base de Datos de Estado de Salud de Proyectos
--  Basado en OMG Essence v1.2 Kernel
--  PostgreSQL
-- ============================================================

-- ─────────────────────────────────────────────
--  CATÁLOGO (datos fijos del estándar)
-- ─────────────────────────────────────────────

CREATE TABLE alpha
(
    id              SERIAL PRIMARY KEY,
    name            VARCHAR(100) NOT NULL,
    area_of_concern VARCHAR(50)  NOT NULL CHECK (area_of_concern IN ('Customer', 'Solution', 'Endeavor')),
    description     TEXT
);

CREATE TABLE alpha_state
(
    id           SERIAL PRIMARY KEY,
    alpha_id     INT          NOT NULL REFERENCES alpha (id),
    state_number SMALLINT     NOT NULL CHECK (state_number >= 1),
    state_name   VARCHAR(100) NOT NULL,
    description  TEXT,
    UNIQUE (alpha_id, state_number)
);



CREATE TABLE state_checklist
(
    id             SERIAL PRIMARY KEY,
    alpha_state_id INT     NOT NULL REFERENCES alpha_state (id),
    criterion_text TEXT    NOT NULL,
    is_mandatory   BOOLEAN NOT NULL DEFAULT TRUE
);
-- ─────────────────────────────────────────────
--  AUTENTICACIÓN
-- ─────────────────────────────────────────────

CREATE TABLE app_user
(
    id            SERIAL PRIMARY KEY,
    email         VARCHAR(200) NOT NULL UNIQUE,
    password_hash TEXT         NOT NULL,
    display_name  VARCHAR(100),
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);
select * from user_session;
CREATE TABLE user_session
(
    id                     SERIAL PRIMARY KEY,
    app_user_id            INT          NOT NULL REFERENCES app_user (id) ON DELETE CASCADE,
    refresh_token_hash     VARCHAR(128) NOT NULL UNIQUE,
    created_at             TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    expires_at             TIMESTAMPTZ  NOT NULL,
    revoked_at             TIMESTAMPTZ,
    replaced_by_token_hash VARCHAR(128)
);

CREATE INDEX ix_user_session_app_user_id ON user_session (app_user_id);

-- ─────────────────────────────────────────────
--  PROYECTOS
-- ─────────────────────────────────────────────
CREATE TABLE project
(
    id           SERIAL PRIMARY KEY,
    user_id      INT           NOT NULL REFERENCES app_user (id) ON DELETE CASCADE,
    name         VARCHAR(200)  NOT NULL,
    description  TEXT,
    phase        VARCHAR(100),
    created_at   TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_project_user_id ON project (user_id);

-- en que nivel de detalle se encuentra cada Alpha dentro de un proyecto (0 = ninguno aún).F
-- Estado actual de cada Alpha dentro de un proyecto.
-- current_state_number = último estado completamente alcanzado (0 = ninguno aún).
CREATE TABLE project_alpha_status
(
    id                   SERIAL PRIMARY KEY,
    project_id           INT         NOT NULL REFERENCES project (id) ON DELETE CASCADE,
    alpha_id             INT         NOT NULL REFERENCES alpha (id),
    current_state_number SMALLINT    NOT NULL DEFAULT 0,
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (project_id, alpha_id)
);

-- Respuestas de checklist por proyecto.
CREATE TABLE checklist_response
(
    id                 SERIAL PRIMARY KEY,
    project_id         INT         NOT NULL REFERENCES project (id) ON DELETE CASCADE,
    state_checklist_id INT         NOT NULL REFERENCES state_checklist (id),
    is_achieved        BOOLEAN     NOT NULL DEFAULT FALSE,
    notes              TEXT,
    updated_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (project_id, state_checklist_id)
);

-- Snapshot del semáforo global en un momento dado.
CREATE TYPE health_status AS ENUM ('Green', 'Yellow', 'Red');

CREATE TABLE health_report
(
    id            SERIAL PRIMARY KEY,
    project_id    INT           NOT NULL REFERENCES project (id) ON DELETE CASCADE,
    created_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    global_status health_status NOT NULL,
    alpha_details JSONB -- detalle por Alpha en el momento del snapshot
);

ALTER TYPE health_status RENAME VALUE 'Green' TO 'green';
ALTER TYPE health_status RENAME VALUE 'Yellow' TO 'yellow';
ALTER TYPE health_status RENAME VALUE 'Red' TO 'red';
-- ─────────────────────────────────────────────
--  ÍNDICES
-- ─────────────────────────────────────────────

CREATE INDEX ix_alpha_state_alpha_id ON alpha_state (alpha_id);
CREATE INDEX ix_state_checklist_state_id ON state_checklist (alpha_state_id);
CREATE INDEX ix_project_alpha_status_project ON project_alpha_status (project_id);
CREATE INDEX ix_checklist_response_project ON checklist_response (project_id);
CREATE INDEX ix_health_report_project ON health_report (project_id, created_at DESC);
CREATE INDEX ix_health_report_details ON health_report USING GIN (alpha_details);

-- ============================================================
--  SEED DATA — 7 Alphas del Kernel con estados y checklists
-- ============================================================

-- ── Alphas ───────────────────────────────────────────────────
INSERT INTO alpha (id, name, area_of_concern, description)
VALUES (1, 'Stakeholders', 'Customer', 'Personas, grupos u organizaciones que afectan o son afectadas por el sistema.'),
       (2, 'Opportunity', 'Customer', 'Contexto que motiva el desarrollo; necesidad de negocio o problema a resolver.'),
       (3, 'Requirements', 'Solution',
        'Lo que el sistema debe hacer para satisfacer las necesidades de los stakeholders.'),
       (4, 'Software System', 'Solution', 'El sistema software que se está desarrollando.'),
       (5, 'Team', 'Endeavor', 'Grupo de personas que desarrollan y mantienen el sistema.'),
       (6, 'Work', 'Endeavor', 'Actividad que involucra esfuerzo mental y/o físico para lograr el objetivo.'),
       (7, 'Way-of-Working', 'Endeavor',
        'Prácticas y herramientas acordadas que el equipo utiliza para guiar su trabajo.');

SELECT setval('alpha_id_seq', (SELECT MAX(id) FROM alpha));

-- ── Alpha 1: Stakeholders ────────────────────────────────────
INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description)
VALUES (101, 1, 1, 'Recognized', 'Se han identificado los stakeholders.'),
       (102, 1, 2, 'Represented', 'Los mecanismos de representación están acordados.'),
       (103, 1, 3, 'Involved', 'Los representantes participan activamente.'),
       (104, 1, 4, 'In Agreement', 'Los stakeholders están de acuerdo en continuar.'),
       (105, 1, 5, 'Satisfied for Deployment', 'Los stakeholders están preparados para aceptar el sistema.'),
       (106, 1, 6, 'Satisfied in Use', 'Los stakeholders están satisfechos con el sistema en producción.');

INSERT INTO state_checklist (alpha_state_id, criterion_text)
VALUES (101, 'Stakeholders del sistema identificados'),
       (102, 'Responsabilidades de representación acordadas'),
       (102, 'Representantes con autoridad acordada'),
       (102, 'Acuerdo sobre el proceso de toma de decisiones'),
       (102, 'Representantes disponibles'),
       (103, 'Representantes disponibles para aclarar dudas'),
       (103, 'Representantes revisan y priorizan defectos'),
       (104, 'Acuerdo sobre los requisitos de alto nivel'),
       (104, 'Acuerdo sobre el alcance del próximo release'),
       (104, 'Acuerdo sobre el release actual'),
       (105, 'Stakeholders satisfechos con el progreso del trabajo'),
       (105, 'Stakeholders de negocio satisfechos con el impacto'),
       (106, 'Stakeholders del sistema satisfechos con el rendimiento'),
       (106, 'Los stakeholders están satisfechos');

-- ── Alpha 2: Opportunity ─────────────────────────────────────
INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description)
VALUES (201, 2, 1, 'Identified', 'Se ha identificado la oportunidad.'),
       (202, 2, 2, 'Solution Needed', 'Se necesita una solución de software.'),
       (203, 2, 3, 'Value Established', 'El valor potencial de la solución está claro.'),
       (204, 2, 4, 'Viable', 'Existe una solución viable.'),
       (205, 2, 5, 'Addressed', 'La solución aborda la oportunidad.'),
       (206, 2, 6, 'Benefit Accrued', 'Se han obtenido los beneficios esperados.');

INSERT INTO state_checklist (alpha_state_id, criterion_text)
VALUES (201, 'Se ha identificado una oportunidad de mejora'),
       (202, 'Al menos un stakeholder quiere aprovecharla'),
       (202, 'Stakeholders de negocio identificados'),
       (202, 'Al menos un stakeholder está comprometido'),
       (203, 'El impacto negativo de no actuar está claro'),
       (203, 'La solución es rentable'),
       (204, 'Una solución puede entregarse y desplegarse'),
       (204, 'Los principales riesgos están identificados'),
       (204, 'Existe una viabilidad técnica razonable'),
       (205, 'La solución satisface la necesidad de negocio'),
       (205, 'El valor de la solución para el negocio está claro'),
       (206, 'La solución produce el beneficio esperado'),
       (206, 'La oportunidad ha sido aprovechada'),
       (206, 'Los stakeholders están satisfechos con la solución');

-- ── Alpha 3: Requirements ────────────────────────────────────
INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description)
VALUES (301, 3, 1, 'Conceived', 'Se conocen los requisitos de alto nivel.'),
       (302, 3, 2, 'Bounded', 'El alcance del sistema está delimitado.'),
       (303, 3, 3, 'Coherent', 'Los requisitos son coherentes.'),
       (304, 3, 4, 'Acceptable', 'Los requisitos son aceptados por los stakeholders.'),
       (305, 3, 5, 'Addressed', 'Los requisitos principales están siendo abordados.'),
       (306, 3, 6, 'Fulfilled', 'Los requisitos están completamente satisfechos.');

INSERT INTO state_checklist (alpha_state_id, criterion_text)
VALUES (301, 'Los stakeholders están de acuerdo con el alcance'),
       (301, 'Los requisitos de alto nivel están identificados'),
       (302, 'Restricciones de alto nivel identificadas'),
       (302, 'Los mecanismos de almacenamiento están identificados'),
       (302, 'El alcance del sistema está acordado con los stakeholders'),
       (302, 'Restricciones técnicas y de negocio identificadas'),
       (303, 'Los requisitos de mayor prioridad están determinados'),
       (303, 'Los requisitos se correlacionan con las necesidades'),
       (303, 'Los requisitos son testables'),
       (304, 'Viabilidad técnica demostrada'),
       (304, 'Los requisitos son lo suficientemente claros'),
       (305, 'Todos los requisitos críticos abordados'),
       (305, 'Los requisitos restantes acordados con stakeholders'),
       (306, 'Los stakeholders aceptan los requisitos implementados'),
       (306, 'El sistema satisface los requisitos');

-- ── Alpha 4: Software System ─────────────────────────────────
INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description)
VALUES (401, 4, 1, 'Architecture Selected', 'Se ha seleccionado la arquitectura.'),
       (402, 4, 2, 'Demonstrable', 'El sistema puede ser demostrado.'),
       (403, 4, 3, 'Usable', 'El sistema puede ser usado.'),
       (404, 4, 4, 'Ready', 'El sistema está listo para desplegarse.'),
       (405, 4, 5, 'Operational', 'El sistema está en operación.'),
       (406, 4, 6, 'Retired', 'El sistema ha sido retirado de uso.');

INSERT INTO state_checklist (alpha_state_id, criterion_text)
VALUES (401, 'Decisiones arquitectónicas tomadas'),
       (401, 'Decisiones de compra-vs-construcción tomadas'),
       (401, 'Riesgos arquitectónicos principales identificados'),
       (402, 'Arquitectura implementada'),
       (402, 'El sistema puede ejecutarse'),
       (402, 'Componentes del sistema identificados'),
       (402, 'Sistema integrable con otros sistemas'),
       (403, 'El sistema es usable'),
       (403, 'Funcionalidad de uso frecuente operativa'),
       (403, 'Requisitos de accesibilidad cumplidos'),
       (403, 'Sistema documentado'),
       (404, 'Impacto sobre stakeholders aceptable'),
       (404, 'Los defectos a corregir están acordados'),
       (404, 'Actividades de despliegue preparadas'),
       (405, 'El sistema está en producción'),
       (405, 'Rendimiento operacional aceptable'),
       (405, 'Vulnerabilidades de seguridad gestionadas'),
       (406, 'El sistema ha sido retirado de producción'),
       (406, 'Datos del sistema conservados según política'),
       (406, 'Plan de reemplazamiento activo');

-- ── Alpha 5: Team ────────────────────────────────────────────
INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description)
VALUES (501, 5, 1, 'Seeded', 'Se han identificado los miembros del equipo.'),
       (502, 5, 2, 'Formed', 'El equipo está formado.'),
       (503, 5, 3, 'Collaborating', 'El equipo está colaborando efectivamente.'),
       (504, 5, 4, 'Performing', 'El equipo está rindiendo al máximo.'),
       (505, 5, 5, 'Adjourned', 'El equipo ha finalizado su trabajo.');

INSERT INTO state_checklist (alpha_state_id, criterion_text)
VALUES (501, 'Restricciones del equipo conocidas'),
       (501, 'Mecanismos de crecimiento del equipo establecidos'),
       (501, 'Responsabilidades y colaboraciones identificadas'),
       (501, 'Facilitador identificado'),
       (502, 'Miembros del equipo identificados'),
       (502, 'Roles del equipo acordados'),
       (502, 'Responsabilidades individuales acordadas'),
       (502, 'Comunicaciones del equipo establecidas'),
       (503, 'Normas de operación acordadas'),
       (503, 'Acceso a recursos y herramientas asegurado'),
       (503, 'Conflictos resueltos'),
       (503, 'Trabajo coordinado eficientemente'),
       (504, 'El equipo responde adaptando su forma de trabajar'),
       (504, 'El equipo identifica y resuelve problemas'),
       (504, 'El equipo logra su misión eficientemente'),
       (505, 'El equipo se ha disuelto'),
       (505, 'Miembros reconocidos por sus contribuciones'),
       (505, 'Experiencias y conocimientos preservados');

-- ── Alpha 6: Work ────────────────────────────────────────────
INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description)
VALUES (601, 6, 1, 'Initiated', 'El trabajo está preparado para comenzar.'),
       (602, 6, 2, 'Prepared', 'El trabajo está preparado.'),
       (603, 6, 3, 'Started', 'El trabajo ha comenzado.'),
       (604, 6, 4, 'Under Control', 'El trabajo está bajo control.'),
       (605, 6, 5, 'Concluded', 'El trabajo ha concluido.'),
       (606, 6, 6, 'Closed', 'El trabajo está cerrado formalmente.');

INSERT INTO state_checklist (alpha_state_id, criterion_text)
VALUES (601, 'Las partes involucradas están de acuerdo en iniciar'),
       (601, 'El enfoque del trabajo está acordado'),
       (601, 'Los criterios de aceptación están acordados'),
       (601, 'Restricciones y riesgos conocidos'),
       (602, 'Estimaciones del trabajo disponibles'),
       (602, 'Plan publicado y disponible'),
       (602, 'Trabajo organizado para su ejecución'),
       (602, 'Medios de financiación confirmados'),
       (603, 'Desarrollo comenzado'),
       (603, 'Actividades de prueba comenzadas'),
       (603, 'Elementos de trabajo gestionados'),
       (604, 'El trabajo sigue el plan acordado'),
       (604, 'El desvío de las estimaciones está controlado'),
       (604, 'Riesgos bajo control'),
       (604, 'Replanificación cuando es necesario'),
       (605, 'Criterios de aceptación cumplidos'),
       (605, 'Stakeholders satisfechos'),
       (606, 'Contrato cerrado'),
       (606, 'Trabajo final aceptado'),
       (606, 'Experiencias documentadas');

-- ── Alpha 7: Way-of-Working ──────────────────────────────────
INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description)
VALUES (701, 7, 1, 'Principles Established', 'Se han establecido los principios de trabajo.'),
       (702, 7, 2, 'Foundation Established', 'Los fundamentos de la forma de trabajar están establecidos.'),
       (703, 7, 3, 'In Use', 'La forma de trabajar está siendo usada.'),
       (704, 7, 4, 'In Place', 'La forma de trabajar está asentada.'),
       (705, 7, 5, 'Working Well', 'La forma de trabajar está funcionando bien.'),
       (706, 7, 6, 'Retired', 'La forma de trabajar ha sido retirada.');

INSERT INTO state_checklist (alpha_state_id, criterion_text)
VALUES (701, 'Necesidades del equipo identificadas'),
       (701, 'Principios y restricciones del equipo identificados'),
       (701, 'Prácticas candidatas identificadas'),
       (701, 'Herramientas candidatas identificadas'),
       (702, 'Enfoque acordado con los stakeholders'),
       (702, 'Herramientas clave acordadas'),
       (702, 'Responsabilidades acordadas'),
       (702, 'Manera de trabajar formada y comunicada'),
       (703, 'Prácticas clave acordadas'),
       (703, 'Herramientas en uso regularmente'),
       (703, 'Uso monitoreado'),
       (704, 'Manera de trabajar usada por todos'),
       (704, 'Dificultades gestionadas'),
       (705, 'Necesidades del equipo cubiertas'),
       (705, 'Mejoras identificadas incorporadas'),
       (705, 'Inspecciones y adaptaciones regulares'),
       (706, 'Equipo reconoce que ya no necesita esta forma de trabajar'),
       (706, 'Experiencias documentadas para futuras referencias');

-- Resetear secuencias tras INSERTs con IDs explícitos
SELECT setval('alpha_state_id_seq', (SELECT MAX(id) FROM alpha_state));
