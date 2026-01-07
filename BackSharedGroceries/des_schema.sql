-- 1. Habilitar la extensión para generar UUIDs automáticamente
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 2. Crear ENUM para el estado del producto (más eficiente que un string)
CREATE TYPE product_status AS ENUM ('Pending', 'InCart', 'Paid');

-- 3. Tabla de FAMILIAS
CREATE TABLE Families (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    invite_code VARCHAR(10) UNIQUE NOT NULL, -- Código corto para el onboarding
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- 4. Tabla de USUARIOS
CREATE TABLE Users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    family_id UUID REFERENCES Families(id) ON DELETE SET NULL, -- Puede ser null al inicio
    current_device_id UUID, -- Para el control de "Single Device Session"
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- 5. Tabla de LISTAS DE COMPRA
CREATE TABLE ShoppingLists (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    family_id UUID NOT NULL REFERENCES Families(id) ON DELETE CASCADE,
    is_active BOOLEAN DEFAULT TRUE, -- Para el borrado lógico/historial de 1 mes
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- 6. Tabla de PRODUCTOS
CREATE TABLE Products (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    list_id UUID NOT NULL REFERENCES ShoppingLists(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    quantity VARCHAR(50), -- Texto para permitir "1kg", "2 piezas", etc.
    status product_status DEFAULT 'Pending',
    last_modified_by_user_id UUID REFERENCES Users(id),
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- 7. Tabla de DISPOSITIVOS (Para Firebase Cloud Messaging)
CREATE TABLE UserDevices (
    user_id UUID PRIMARY KEY REFERENCES Users(id) ON DELETE CASCADE,
    fcm_token TEXT NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- 8. Índices para optimizar el rendimiento en dispositivos de gama baja
-- (Acelera las consultas de sincronización y búsqueda de sugerencias)

CREATE INDEX idx_products_list_id ON Products(list_id);
CREATE INDEX idx_products_updated_at ON Products(updated_at);
CREATE INDEX idx_shopping_lists_family_id ON ShoppingLists(family_id);
CREATE INDEX idx_families_invite_code ON Families(invite_code);

-- Función para actualizar el campo updated_at automáticamente en cada cambio
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_products_modtime
    BEFORE UPDATE ON Products
    FOR EACH ROW
    EXECUTE PROCEDURE update_updated_at_column();