

-- 1. Create the database if it doesn't exist
CREATE DATABASE IF NOT EXISTS cybersecbot;

-- 2. Select it
USE cybersecbot;

-- 3. Create the Tasks table
CREATE TABLE IF NOT EXISTS Tasks (
    Id           INT AUTO_INCREMENT PRIMARY KEY,
    Title        VARCHAR(255) NOT NULL,
    Description  TEXT,
    IsCompleted  TINYINT(1)   NOT NULL DEFAULT 0,
    ReminderDate DATETIME     NULL,
    CreatedAt    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 4. Seed a sample task
INSERT INTO Tasks (Title, Description, IsCompleted, ReminderDate, CreatedAt)
VALUES (
    'Enable Two-Factor Authentication',
    'Set up 2FA on all your important accounts — email, banking, and social media.',
    0,
    DATE_ADD(NOW(), INTERVAL 3 DAY),
    NOW()
);

-- 5. Confirm
SELECT 'Database setup complete.' AS Status;
SELECT * FROM Tasks;