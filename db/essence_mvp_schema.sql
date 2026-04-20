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
-- Nota: valores en minúsculas para compatibilidad con el traductor de nombres de Npgsql.
CREATE TYPE health_status AS ENUM ('green', 'yellow', 'red');

CREATE TABLE health_report
(
    id            SERIAL PRIMARY KEY,
    project_id    INT           NOT NULL REFERENCES project (id) ON DELETE CASCADE,
    created_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    global_status health_status NOT NULL,
    alpha_details JSONB
);

-- ─────────────────────────────────────────────
--  ÍNDICES
-- ─────────────────────────────────────────────

CREATE INDEX ix_alpha_state_alpha_id ON alpha_state (alpha_id);
CREATE INDEX ix_state_checklist_state_id ON state_checklist (alpha_state_id);
CREATE INDEX ix_project_alpha_status_project ON project_alpha_status (project_id);
CREATE INDEX ix_checklist_response_project ON checklist_response (project_id);
CREATE INDEX ix_health_report_project ON health_report (project_id, created_at DESC);
CREATE INDEX ix_health_report_details ON health_report USING GIN (alpha_details);

-- ─────────────────────────────────────────────
--  SEED DATA — OMG Essence v1.2 Kernel
-- ─────────────────────────────────────────────

-- 7 Alphas
INSERT INTO alpha (id, name, area_of_concern, description) VALUES
(1, 'Stakeholders',    'Customer', 'The people, groups, or organizations who affect or are affected by the software engineering endeavor.'),
(2, 'Opportunity',     'Customer', 'The set of circumstances that makes it appropriate to develop or change a software system.'),
(3, 'Requirements',    'Customer', 'What the software system must do to address the opportunity and satisfy the stakeholders.'),
(4, 'Software System', 'Solution', 'A system made up of software, hardware, and data that provides its primary value by the execution of the software.'),
(5, 'Team',            'Endeavor', 'The group of people actively engaged in the development, maintenance, delivery and support of a specific software system.'),
(6, 'Work',            'Endeavor', 'Activity involving mental or physical effort done in order to achieve a result.'),
(7, 'Way-of-Working',  'Endeavor', 'The tailored set of practices and tools used by a team to guide and support their work.');

SELECT setval(pg_get_serial_sequence('alpha', 'id'), 7);

-- ─── Alpha 1: Stakeholders (6 states) ───

INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description) VALUES
(1,  1, 1, 'Recognized',                'Stakeholders have been identified and their involvement agreed.'),
(2,  1, 2, 'Represented',               'Mechanisms to involve all stakeholders are in place.'),
(3,  1, 3, 'Involved',                  'Stakeholder representatives are actively engaged in the work.'),
(4,  1, 4, 'In Agreement',              'Stakeholders agree on the requirements and how they have been met.'),
(5,  1, 5, 'Satisfied for Deployment',  'Stakeholders are satisfied with the system prior to deployment.'),
(6,  1, 6, 'Satisfied in Use',          'Stakeholders are satisfied with the deployed and operational system.');

-- ─── Alpha 2: Opportunity (6 states) ───

INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description) VALUES
(7,  2, 1, 'Identified',       'An opportunity for a new or improved system has been identified.'),
(8,  2, 2, 'Solution Needed',  'The need for a software-based solution has been established.'),
(9,  2, 3, 'Value Established','The value a solution would provide has been established.'),
(10, 2, 4, 'Viable',           'It is agreed that a solution can be produced within acceptable terms.'),
(11, 2, 5, 'Addressed',        'A solution has been produced that demonstrably addresses the opportunity.'),
(12, 2, 6, 'Benefit Accrued',  'The operational benefits are being accrued by the customer.');

-- ─── Alpha 3: Requirements (6 states) ───

INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description) VALUES
(13, 3, 1, 'Conceived',    'The need for a new system has been agreed.'),
(14, 3, 2, 'Bounded',      'The purpose and scope of the new system are clear.'),
(15, 3, 3, 'Coherent',     'Requirements consistently describe the essential characteristics of the system.'),
(16, 3, 4, 'Acceptable',   'The requirements describe a system acceptable to the stakeholders.'),
(17, 3, 5, 'Addressed',    'Sufficient requirements have been addressed to produce a usable system.'),
(18, 3, 6, 'Fulfilled',    'The requirements fully satisfy the need for the system.');

-- ─── Alpha 4: Software System (6 states) ───

INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description) VALUES
(19, 4, 1, 'Architecture Selected', 'An architecture has been selected that addresses the key technical risks.'),
(20, 4, 2, 'Demonstrable',          'Key use cases of the system can be demonstrated.'),
(21, 4, 3, 'Usable',                'The system is usable by all intended stakeholders.'),
(22, 4, 4, 'Ready',                 'The system is ready for deployment.'),
(23, 4, 5, 'Operational',           'The system is deployed and in use in an operational environment.'),
(24, 4, 6, 'Retired',               'The system is no longer supported.');

-- ─── Alpha 5: Team (5 states) ───

INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description) VALUES
(25, 5, 1, 'Seeded',        'The team has been seeded with appropriate skills and competencies.'),
(26, 5, 2, 'Formed',        'The team is ready to start the work.'),
(27, 5, 3, 'Collaborating', 'The team members are working together as a team.'),
(28, 5, 4, 'Performing',    'The team consistently delivers good results and meets its commitments.'),
(29, 5, 5, 'Adjourned',     'The team is no longer accountable for the outcomes.');

-- ─── Alpha 6: Work (6 states) ───

INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description) VALUES
(30, 6, 1, 'Initiated',     'The work has been requested and the result required is clear.'),
(31, 6, 2, 'Prepared',      'The work is ready to be started.'),
(32, 6, 3, 'Started',       'The work is underway.'),
(33, 6, 4, 'Under Control', 'The work is progressing well and is manageable.'),
(34, 6, 5, 'Concluded',     'All work to produce the key artifacts has been completed.'),
(35, 6, 6, 'Closed',        'All outstanding commitments have been met.');

-- ─── Alpha 7: Way-of-Working (6 states) ───

INSERT INTO alpha_state (id, alpha_id, state_number, state_name, description) VALUES
(36, 7, 1, 'Principles Established', 'Principles and constraints shaping the way-of-working are established.'),
(37, 7, 2, 'Foundation Established', 'The key practices and tools forming the foundation are established.'),
(38, 7, 3, 'In Use',                 'The way-of-working is in use by the team to do real work.'),
(39, 7, 4, 'In Place',               'The way-of-working is well established and consistently followed.'),
(40, 7, 5, 'Working Well',           'The way-of-working is working well for the team.'),
(41, 7, 6, 'Retired',                'The way-of-working is no longer in use by the team.');

SELECT setval(pg_get_serial_sequence('alpha_state', 'id'), 41);

-- ─────────────────────────────────────────────
--  CHECKLISTS (IDs start at 1001)
-- ─────────────────────────────────────────────

-- Stakeholders — State 1: Recognized
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1001, 1, 'All stakeholder groups that are, or will be, affected by the development have been identified.'),
(1002, 1, 'There is agreement on the stakeholder groups to be represented.'),
(1003, 1, 'The responsibilities of the stakeholder representatives have been defined.');

-- Stakeholders — State 2: Represented
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1004, 2, 'The mechanisms for involving the stakeholders and their representatives have been agreed upon.'),
(1005, 2, 'The stakeholder representatives have been identified.'),
(1006, 2, 'The stakeholder representatives have agreed to take on their responsibilities.');

-- Stakeholders — State 3: Involved
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1007, 3, 'The stakeholder representatives are actively involved in the work.'),
(1008, 3, 'The stakeholder representatives and the team are aligned on the expectations for the system.'),
(1009, 3, 'The stakeholder representatives provide feedback and take part in decision-making in a timely manner.');

-- Stakeholders — State 4: In Agreement
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1010, 4, 'The stakeholder representatives are in agreement with the requirements as captured.'),
(1011, 4, 'The stakeholder representatives are in agreement with how their input has been used.'),
(1012, 4, 'All concerns raised by the stakeholders have been addressed.');

-- Stakeholders — State 5: Satisfied for Deployment
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1013, 5, 'The stakeholder representatives are satisfied with the system as deployed.'),
(1014, 5, 'The system has passed the appropriate acceptance tests.'),
(1015, 5, 'Feedback from the stakeholders on the system has been addressed.');

-- Stakeholders — State 6: Satisfied in Use
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1016, 6, 'Stakeholders are actively using the system and are satisfied with it.'),
(1017, 6, 'The stakeholders are getting the business benefits expected of the system.'),
(1018, 6, 'Issues raised during use have been addressed.');

-- Opportunity — State 1: Identified
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1019, 7, 'An idea for improving current ways of working or applying a new software system has been identified.'),
(1020, 7, 'At least one stakeholder wishes to invest in better understanding the opportunity.'),
(1021, 7, 'The other stakeholders who may be affected have been identified.');

-- Opportunity — State 2: Solution Needed
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1022, 8, 'The need for a software-based solution has been confirmed.'),
(1023, 8, 'At least one possible solution has been identified.'),
(1024, 8, 'The key stakeholders required for the solution are known.');

-- Opportunity — State 3: Value Established
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1025, 9, 'The value of a successful solution has been established.'),
(1026, 9, 'The success criteria by which the opportunity will be judged have been agreed upon.'),
(1027, 9, 'The size of the opportunity has been established.');

-- Opportunity — State 4: Viable
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1028, 10, 'A solution has been outlined.'),
(1029, 10, 'The solution can be developed and deployed within acceptable time, risk and cost.'),
(1030, 10, 'The risks associated with the opportunity have been identified and are manageable.');

-- Opportunity — State 5: Addressed
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1031, 11, 'A solution has been developed that demonstrably addresses the opportunity.'),
(1032, 11, 'The stakeholder representatives confirm that the solution addresses the opportunity.'),
(1033, 11, 'The business case, if required, has been updated.');

-- Opportunity — State 6: Benefit Accrued
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1034, 12, 'The system has been made available to the stakeholders and they are getting the benefits.'),
(1035, 12, 'The return on investment has been achieved, or is on track to be achieved.'),
(1036, 12, 'The opportunity is no longer driving development of the system.');

-- Requirements — State 1: Conceived
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1037, 13, 'The need for a new system has been agreed.'),
(1038, 13, 'The proposed system has been described.'),
(1039, 13, 'The key stakeholders for the new system have been identified.');

-- Requirements — State 2: Bounded
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1040, 14, 'The purpose and theme of the new system are clear.'),
(1041, 14, 'The system to be produced has been scoped.'),
(1042, 14, 'Requirements exist that define the most important features and functions of the system.');

-- Requirements — State 3: Coherent
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1043, 15, 'Requirements provide a consistent description of the essential characteristics of the system.'),
(1044, 15, 'The requirements adequately reflect the business needs of the stakeholders.'),
(1045, 15, 'The priority of the requirements has been established.');

-- Requirements — State 4: Acceptable
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1046, 16, 'The requirements describe a system that is acceptable to the stakeholders.'),
(1047, 16, 'The rate of change to the agreed requirements is low and under control.'),
(1048, 16, 'Stakeholder acceptance criteria for the system are clear.');

-- Requirements — State 5: Addressed
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1049, 17, 'Sufficient requirements have been addressed to produce a usable system.'),
(1050, 17, 'The system can be operated and supported based on the requirements addressed.'),
(1051, 17, 'The stakeholders accept the requirements as addressed.');

-- Requirements — State 6: Fulfilled
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1052, 18, 'The system fully addresses the requirements as agreed with the stakeholders.'),
(1053, 18, 'There are no gaps in the requirements met by the system.'),
(1054, 18, 'The stakeholders confirm that the requirements have been fulfilled.');

-- Software System — State 1: Architecture Selected
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1055, 19, 'The criteria to be used when selecting the architecture have been agreed on.'),
(1056, 19, 'Hardware platforms have been identified.'),
(1057, 19, 'Programming languages and technologies to be used have been selected.');

-- Software System — State 2: Demonstrable
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1058, 20, 'Key use cases of the system can be demonstrated.'),
(1059, 20, 'The system can be operated by stakeholders focused on the quality of the key use cases.'),
(1060, 20, 'The architecture of the system has been validated.');

-- Software System — State 3: Usable
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1061, 21, 'The system can be operated by all intended stakeholders.'),
(1062, 21, 'All key quality characteristics are sufficiently addressed for the system to be used.'),
(1063, 21, 'The system has been accepted for operational use by all stakeholders.');

-- Software System — State 4: Ready
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1064, 22, 'The system is ready for deployment.'),
(1065, 22, 'All defects preventing deployment have been removed.'),
(1066, 22, 'The stakeholders have accepted the system for deployment.');

-- Software System — State 5: Operational
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1067, 23, 'The system has been deployed and is in use in an operational environment.'),
(1068, 23, 'The system is available for use by the stakeholders.'),
(1069, 23, 'Agreed service levels are being met.');

-- Software System — State 6: Retired
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1070, 24, 'The system has been replaced or discontinued.'),
(1071, 24, 'The system is no longer supported.'),
(1072, 24, 'All stakeholders have been notified of the retirement.');

-- Team — State 1: Seeded
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1073, 25, 'The team''s mission is clear even if details are not fully defined.'),
(1074, 25, 'Enough staff are available to start the work.'),
(1075, 25, 'The skills needed to perform the work are identified.');

-- Team — State 2: Formed
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1076, 26, 'Individual responsibilities are understood.'),
(1077, 26, 'Team members are accepting work.'),
(1078, 26, 'The team is ready to accept new work.');

-- Team — State 3: Collaborating
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1079, 27, 'The team members know each other.'),
(1080, 27, 'The team members trust each other.'),
(1081, 27, 'Team communication is open and honest.');

-- Team — State 4: Performing
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1082, 28, 'The team consistently meets its commitments.'),
(1083, 28, 'The team continually adapts to the changing context.'),
(1084, 28, 'The team identifies and addresses problems without outside help.');

-- Team — State 5: Adjourned
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1085, 29, 'The team has been officially disbanded.'),
(1086, 29, 'The team members have been reassigned to other duties or have left the organization.'),
(1087, 29, 'Lessons learned have been communicated and documented.');

-- Work — State 1: Initiated
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1088, 30, 'The result required of the work is clear.'),
(1089, 30, 'Any constraints on the work are clearly identified.'),
(1090, 30, 'The stakeholders that fund the work are known.');

-- Work — State 2: Prepared
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1091, 31, 'Commitment is made.'),
(1092, 31, 'The required funds are secured.'),
(1093, 31, 'The work is broken down sufficiently for productive work to start.');

-- Work — State 3: Started
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1094, 32, 'Development work has been started.'),
(1095, 32, 'Work progress is monitored.'),
(1096, 32, 'The team is working on agreed deliverables.');

-- Work — State 4: Under Control
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1097, 33, 'Work is going well against its target progress.'),
(1098, 33, 'Remaining work is understood and manageable.'),
(1099, 33, 'Work progress is monitored at appropriate intervals.');

-- Work — State 5: Concluded
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1100, 34, 'All work to produce the system''s key artifacts has been completed.'),
(1101, 34, 'The transition to the next phase of work, or to operations, has been made.'),
(1102, 34, 'Lessons learned have been documented and shared.');

-- Work — State 6: Closed
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1103, 35, 'All outstanding commitments have been met or their disposition has been agreed with stakeholders.'),
(1104, 35, 'The team has been disbanded or reallocated.'),
(1105, 35, 'Any outstanding financial matters have been resolved.');

-- Way-of-Working — State 1: Principles Established
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1106, 36, 'Principles and constraints that shape the way-of-working are established.'),
(1107, 36, 'The team follows the principles established.'),
(1108, 36, 'The practices to follow are agreed on by the team.');

-- Way-of-Working — State 2: Foundation Established
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1109, 37, 'The key practices and tools that form the foundation of the way of working are established.'),
(1110, 37, 'The team commits to working in accordance with the way-of-working.'),
(1111, 37, 'Enough practices for work to begin are in place.');

-- Way-of-Working — State 3: In Use
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1112, 38, 'The way-of-working is in use by the team to do real work.'),
(1113, 38, 'All team members are using the way-of-working to accomplish their work.'),
(1114, 38, 'The effectiveness of the way-of-working is being assessed.');

-- Way-of-Working — State 4: In Place
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1115, 39, 'The team''s way of working is well established and understood.'),
(1116, 39, 'The way-of-working is continuously updated and improved.'),
(1117, 39, 'Deviations from the way-of-working are addressed.');

-- Way-of-Working — State 5: Working Well
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1118, 40, 'The team''s way of working is working well for the team.'),
(1119, 40, 'Continuous improvement of the way-of-working is in place.'),
(1120, 40, 'The way-of-working is consistently followed by the team.');

-- Way-of-Working — State 6: Retired
INSERT INTO state_checklist (id, alpha_state_id, criterion_text) VALUES
(1121, 41, 'The way-of-working is no longer in use by the team.'),
(1122, 41, 'The practices and tools used are no longer needed.'),
(1123, 41, 'Lessons learned from using the way-of-working have been documented.');

SELECT setval(pg_get_serial_sequence('state_checklist', 'id'), 1123);
