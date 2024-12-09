CREATE DATABASE IF NOT EXISTS `azienda_api`; -- 👈 da aggiungere allo script
USE `azienda_api`; -- 👈 da aggiungere allo script

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Aziende` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nome` nvarchar(100) NOT NULL,
    `Indirizzo` nvarchar(100) NULL,
    CONSTRAINT `PK_Aziende` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Prodotti` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `AziendaId` int NOT NULL,
    `Nome` nvarchar(100) NOT NULL,
    `Descrizione` nvarchar(200) NULL,
    CONSTRAINT `PK_Prodotti` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Prodotti_Aziende_AziendaId` FOREIGN KEY (`AziendaId`) REFERENCES `Aziende` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Sviluppatori` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `AziendaId` int NOT NULL,
    `Nome` nvarchar(40) NOT NULL,
    `Cognome` nvarchar(40) NOT NULL,
    CONSTRAINT `PK_Sviluppatori` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Sviluppatori_Aziende_AziendaId` FOREIGN KEY (`AziendaId`) REFERENCES `Aziende` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `SviluppaProdotti` (
    `ProdottoId` int NOT NULL,
    `SviluppatoreId` int NOT NULL,
    CONSTRAINT `PK_SviluppaProdotti` PRIMARY KEY (`SviluppatoreId`, `ProdottoId`),
    CONSTRAINT `FK_SviluppaProdotti_Prodotti_ProdottoId` FOREIGN KEY (`ProdottoId`) REFERENCES `Prodotti` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_SviluppaProdotti_Sviluppatori_SviluppatoreId` FOREIGN KEY (`SviluppatoreId`) REFERENCES `Sviluppatori` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

INSERT INTO `Aziende` (`Id`, `Indirizzo`, `Nome`)
VALUES (1, 'One Microsoft Way, Redmond, WA 98052, Stati Uniti', 'Microsoft'),
(2, '1600 Amphitheatre Pkwy, Mountain View, CA 94043, Stati Uniti', 'Google'),
(3, '1 Apple Park Way Cupertino, California, 95014-0642 United States', 'Apple');

INSERT INTO `Prodotti` (`Id`, `AziendaId`, `Descrizione`, `Nome`)
VALUES (1, 1, 'Applicazione per la gestione delle Note', 'SuperNote'),
(2, 1, 'Applicazione per la visione di film in streaming', 'My Cinema'),
(3, 2, 'Applicazione per il cad 3d', 'SuperCad');

INSERT INTO `Sviluppatori` (`Id`, `AziendaId`, `Cognome`, `Nome`)
VALUES (1, 1, 'Rossi', 'Mario'),
(2, 1, 'Verdi', 'Giulio'),
(3, 2, 'Bianchi', 'Leonardo');

INSERT INTO `SviluppaProdotti` (`ProdottoId`, `SviluppatoreId`)
VALUES (1, 1),
(1, 2),
(3, 3);

CREATE INDEX `IX_Prodotti_AziendaId` ON `Prodotti` (`AziendaId`);

CREATE INDEX `IX_SviluppaProdotti_ProdottoId` ON `SviluppaProdotti` (`ProdottoId`);

CREATE INDEX `IX_Sviluppatori_AziendaId` ON `Sviluppatori` (`AziendaId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20241129084855_InitialMigrate', '9.0.0-preview.1.24081.2');

COMMIT;

