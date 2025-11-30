-- ============================================================
-- SCRIPT SQL PARA AURORA POSTGRESQL
-- Database: TCS-XSA
-- Host: tcsxesan.cdupy54qjkk5.us-east-1.rds.amazonaws.com:5432
-- ============================================================

-- ============================================================
-- ROLES
-- ============================================================
CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO roles (nombre) VALUES 
('general'),
('analista');

-- ============================================================
-- USUARIOS
-- ============================================================
CREATE TABLE usuarios (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    email VARCHAR(120) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    rol_id INT NOT NULL REFERENCES roles(id),
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================
-- TIPOS SLA
-- ============================================================
CREATE TABLE tipos_sla (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(10) NOT NULL UNIQUE,   -- SLA1 / SLA2
    descripcion TEXT
);

INSERT INTO tipos_sla (codigo, descripcion) VALUES
('SLA1', 'Nuevo personal solicitado (< 35 días)'),
('SLA2', 'Reemplazo de personal (< 20 días)');

-- ============================================================
-- SOLICITUDES
-- ============================================================
CREATE TABLE solicitudes (
    id SERIAL PRIMARY KEY,
    rol VARCHAR(50) NOT NULL,
    fecha_solicitud DATE NOT NULL,
    fecha_ingreso DATE NOT NULL,
    tipo_sla_id INT NOT NULL REFERENCES tipos_sla(id),
    creado_por INT REFERENCES usuarios(id),
    fecha_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================
-- LOG DE ACCESO (Auditoría y seguridad corporativa)
-- ============================================================
CREATE TABLE log_acceso (
    id SERIAL PRIMARY KEY,
    usuario_id INT REFERENCES usuarios(id),
    metodo VARCHAR(10),
    endpoint VARCHAR(200),
    accion VARCHAR(200),
    ip VARCHAR(50),
    user_agent TEXT,
    fecha TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_log_usuario ON log_acceso(usuario_id);
CREATE INDEX idx_log_fecha ON log_acceso(fecha DESC);

-- ============================================================
-- ALERTAS (Notificaciones automáticas de SLA crítico)
-- ============================================================
CREATE TABLE IF NOT EXISTS alertas (
    id SERIAL PRIMARY KEY,
    rol VARCHAR(100),
    porcentaje DOUBLE PRECISION,
    mensaje TEXT,
    fecha TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    atendida BOOLEAN DEFAULT FALSE
);

CREATE INDEX idx_alertas_fecha ON alertas(fecha DESC);
CREATE INDEX idx_alertas_atendida ON alertas(atendida);
