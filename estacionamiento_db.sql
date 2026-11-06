-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 06-11-2026 a las 22:25:50
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `estacionamiento_db`
--
CREATE DATABASE IF NOT EXISTS `estacionamiento_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `estacionamiento_db`;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `abono`
--

CREATE TABLE `abono` (
  `id` int(11) NOT NULL,
  `id_cliente` int(11) NOT NULL,
  `id_vehiculo` int(11) NOT NULL,
  `fecha_inicio` date NOT NULL,
  `fecha_fin` date NOT NULL,
  `precio` decimal(10,2) NOT NULL,
  `estado` tinyint(1) NOT NULL DEFAULT 1,
  `activo` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `abono`
--

INSERT INTO `abono` (`id`, `id_cliente`, `id_vehiculo`, `fecha_inicio`, `fecha_fin`, `precio`, `estado`, `activo`) VALUES
(1, 1, 1, '2026-09-15', '2026-11-15', 90000.00, 1, 1),
(2, 2, 2, '2026-10-01', '2026-12-31', 90000.00, 1, 1),
(3, 3, 3, '2026-07-01', '2026-07-31', 90000.00, 1, 1),
(4, 4, 4, '2026-09-01', '2026-12-01', 250000.00, 0, 1),
(5, 5, 5, '2026-09-20', '2026-10-20', 45000.00, 1, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `cliente`
--

CREATE TABLE `cliente` (
  `id` int(11) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `apellido` varchar(50) NOT NULL,
  `dni` varchar(20) NOT NULL,
  `telefono` varchar(30) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `cliente`
--

INSERT INTO `cliente` (`id`, `nombre`, `apellido`, `dni`, `telefono`, `email`, `activo`) VALUES
(1, 'Lucía', 'Fernández', '21358014', '2664007919', 'lucia.fernandez@mail.com', 1),
(2, 'Martín', 'Gómez', '22716028', '2664015838', 'martin.gomez@mail.com', 1),
(3, 'Sofía', 'Rodríguez', '24074042', '2664023757', 'sofia.rodriguez@mail.com', 1),
(4, 'Julián', 'López', '25432056', '2664031676', 'julian.lopez@mail.com', 1),
(5, 'Camila', 'Martínez', '26790070', '2664039595', 'camila.martinez@mail.com', 1),
(6, 'Mateo', 'Pérez', '28148084', '2664047514', 'mateo.perez@mail.com', 1),
(7, 'Valentina', 'Sánchez', '29506098', '2664055433', 'valentina.sanchez@mail.com', 1),
(8, 'Nicolás', 'Romero', '30864112', '2664063352', 'nicolas.romero@mail.com', 1),
(9, 'Agustina', 'Díaz', '32222126', '2664071271', 'agustina.diaz@mail.com', 1),
(10, 'Facundo', 'Torres', '33580140', '2664079190', 'facundo.torres@mail.com', 1),
(11, 'Florencia', 'Álvarez', '34938154', '2664087109', 'florencia.alvarez@mail.com', 1),
(12, 'Tomás', 'Ruiz', '21296168', '2664095028', 'tomas.ruiz@mail.com', 1),
(13, 'Julieta', 'Acosta', '22654182', '2664102947', 'julieta.acosta@mail.com', 1),
(14, 'Santiago', 'Benítez', '24012196', '2664110866', 'santiago.benitez@mail.com', 1),
(15, 'Micaela', 'Herrera', '25370210', '2664118785', 'micaela.herrera@mail.com', 1),
(16, 'Lautaro', 'Medina', '26728224', '2664126704', 'lautaro.medina@mail.com', 1),
(17, 'Candela', 'Castro', '28086238', '2664134623', 'candela.castro@mail.com', 1),
(18, 'Bruno', 'Ortiz', '29444252', '2664142542', 'bruno.ortiz@mail.com', 1),
(19, 'Rocío', 'Silva', '30802266', '2664150461', 'rocio.silva@mail.com', 1),
(20, 'Franco', 'Molina', '32160280', '2664158380', 'franco.molina@mail.com', 1),
(21, 'Carolina', 'Vega', '33518294', '2664166299', 'carolina.vega@mail.com', 1),
(22, 'Emiliano', 'Ríos', '34876308', '2664174218', 'emiliano.rios@mail.com', 1),
(23, 'Daniela', 'Suárez', '21234322', '2664182137', 'daniela.suarez@mail.com', 1),
(24, 'Gonzalo', 'Navarro', '22592336', '2664190056', 'gonzalo.navarro@mail.com', 1),
(25, 'Paula', 'Cabrera', '23950350', '2664197975', 'paula.cabrera@mail.com', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `cochera`
--

CREATE TABLE `cochera` (
  `id` int(11) NOT NULL,
  `numero` varchar(20) NOT NULL,
  `estado` varchar(30) NOT NULL DEFAULT 'Libre',
  `id_tipo_vehiculo` int(11) NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `cochera`
--

INSERT INTO `cochera` (`id`, `numero`, `estado`, `id_tipo_vehiculo`, `activo`) VALUES
(1, 'A01', 'Ocupada', 1, 1),
(2, 'A02', 'Libre', 1, 1),
(3, 'A03', 'Libre', 1, 1),
(4, 'A04', 'Libre', 1, 1),
(5, 'A05', 'Libre', 1, 1),
(6, 'A06', 'Libre', 1, 1),
(7, 'A07', 'Libre', 1, 1),
(8, 'A08', 'Libre', 1, 1),
(9, 'A09', 'Libre', 1, 1),
(10, 'A10', 'Libre', 1, 1),
(11, 'A11', 'Libre', 1, 1),
(12, 'A12', 'Libre', 1, 1),
(13, 'M01', 'Ocupada', 2, 1),
(14, 'M02', 'Libre', 2, 1),
(15, 'M03', 'Libre', 2, 1),
(16, 'M04', 'Libre', 2, 1),
(17, 'M05', 'Libre', 2, 1),
(18, 'M06', 'Libre', 2, 1),
(19, 'M07', 'Libre', 2, 1),
(20, 'M08', 'Libre', 2, 1),
(21, 'C01', 'Ocupada', 3, 1),
(22, 'C02', 'Libre', 3, 1),
(23, 'C03', 'Libre', 3, 1),
(24, 'C04', 'Libre', 3, 1),
(25, 'C05', 'Libre', 3, 1),
(26, 'C06', 'Libre', 3, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `estadia`
--

CREATE TABLE `estadia` (
  `id` int(11) NOT NULL,
  `id_vehiculo` int(11) NOT NULL,
  `id_cochera` int(11) NOT NULL,
  `id_tarifa` int(11) DEFAULT NULL,
  `id_abono` int(11) DEFAULT NULL,
  `id_usuario` int(11) NOT NULL,
  `fecha_hora_ingreso` datetime NOT NULL,
  `fecha_hora_egreso` datetime DEFAULT NULL,
  `estado` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `estadia`
--

INSERT INTO `estadia` (`id`, `id_vehiculo`, `id_cochera`, `id_tarifa`, `id_abono`, `id_usuario`, `fecha_hora_ingreso`, `fecha_hora_egreso`, `estado`) VALUES
(1, 6, 1, 1, NULL, 7, '2026-10-04 10:08:08', '2026-10-04 13:13:08', 0),
(2, 7, 4, 1, NULL, 7, '2026-10-04 15:08:08', '2026-10-04 16:43:08', 0),
(3, 8, 13, 3, NULL, 6, '2026-10-04 18:28:08', '2026-10-04 23:48:08', 0),
(4, 10, 5, 1, NULL, 7, '2026-10-05 09:28:08', '2026-10-05 19:38:08', 0),
(5, 11, 21, 5, NULL, 7, '2026-10-05 12:48:08', '2026-10-05 15:18:08', 0),
(6, 12, 6, 2, NULL, 6, '2026-10-05 21:08:08', '2026-10-07 04:48:08', 0),
(7, 13, 7, 1, NULL, 7, '2026-10-06 03:48:08', '2026-10-06 04:33:08', 0),
(8, 14, 14, 3, NULL, 7, '2026-10-06 12:08:08', '2026-10-06 16:08:08', 0),
(9, 15, 8, 1, NULL, 7, '2026-10-06 18:48:08', '2026-10-07 03:08:08', 0),
(10, 16, 22, 6, NULL, 6, '2026-10-07 01:28:08', '2026-10-09 03:28:08', 0),
(11, 17, 9, 1, NULL, 7, '2026-10-07 08:08:08', '2026-10-07 10:18:08', 0),
(12, 18, 10, 1, NULL, 7, '2026-10-07 16:28:08', '2026-10-08 07:28:08', 0),
(13, 19, 15, 3, NULL, 7, '2026-10-07 23:08:08', '2026-10-08 00:23:08', 0),
(14, 20, 11, 1, NULL, 7, '2026-10-08 05:48:08', '2026-10-08 10:08:08', 0),
(15, 21, 23, 5, NULL, 6, '2026-10-08 14:08:08', '2026-10-08 21:08:08', 0),
(16, 22, 12, 2, NULL, 7, '2026-10-08 22:28:08', '2026-10-09 23:28:08', 0),
(17, 23, 1, 1, NULL, 7, '2026-10-09 06:48:08', '2026-10-09 08:38:08', 0),
(18, 24, 16, 3, NULL, 7, '2026-10-09 15:08:08', '2026-10-09 18:28:08', 0),
(19, 2, 2, NULL, 2, 7, '2026-10-07 21:28:08', '2026-10-08 02:28:08', 0),
(20, 1, 3, NULL, 1, 7, '2026-10-09 20:08:08', '2026-10-10 02:08:08', 0),
(21, 26, 17, 3, NULL, 6, '2026-10-10 01:08:08', '2026-10-10 04:08:08', 0),
(22, 1, 3, NULL, 1, 6, '2026-10-10 14:33:08', '2026-10-10 21:28:26', 0),
(23, 5, 13, NULL, 5, 7, '2026-10-10 12:38:08', '2026-10-10 17:21:32', 0),
(24, 6, 2, 1, NULL, 7, '2026-10-10 15:28:08', '2026-10-10 21:29:35', 0),
(25, 8, 14, 3, NULL, 7, '2026-10-10 10:48:08', '2026-10-10 16:21:50', 0),
(26, 11, 21, 5, NULL, 7, '2026-10-10 15:43:08', '2026-10-10 17:21:57', 0),
(27, 25, 4, 1, NULL, 6, '2026-10-10 06:08:08', '2026-10-10 16:12:39', 0),
(28, 19, 13, 3, NULL, 6, '2026-10-10 17:25:56', '2026-10-10 17:58:32', 0),
(29, 25, 1, 2, NULL, 7, '2026-10-10 21:27:39', '2026-10-30 19:40:21', 0),
(30, 1, 1, NULL, 1, 6, '2026-11-06 18:20:50', NULL, 1),
(31, 5, 13, NULL, NULL, 6, '2026-11-06 18:21:27', NULL, 1),
(32, 11, 21, NULL, NULL, 6, '2026-11-06 18:22:20', NULL, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `pago`
--

CREATE TABLE `pago` (
  `id` int(11) NOT NULL,
  `id_estadia` int(11) NOT NULL,
  `id_usuario_registro` int(11) NOT NULL,
  `fecha_hora_pago` datetime NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `metodo_pago` varchar(50) NOT NULL,
  `estado` tinyint(1) NOT NULL DEFAULT 1,
  `comprobante` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `pago`
--

INSERT INTO `pago` (`id`, `id_estadia`, `id_usuario_registro`, `fecha_hora_pago`, `monto`, `metodo_pago`, `estado`, `comprobante`) VALUES
(1, 1, 7, '2026-10-04 13:13:09', 6000.00, 'Efectivo', 1, NULL),
(2, 2, 7, '2026-10-04 16:43:09', 3000.00, 'Tarjeta de débito', 1, NULL),
(3, 3, 6, '2026-10-04 23:48:09', 4800.00, 'Efectivo', 1, NULL),
(4, 4, 7, '2026-10-05 19:38:09', 16500.00, 'Transferencia', 0, NULL),
(5, 5, 7, '2026-10-05 15:18:09', 6600.00, 'Efectivo', 1, NULL),
(6, 6, 6, '2026-10-07 04:48:09', 24000.00, 'Transferencia', 1, NULL),
(7, 7, 7, '2026-10-06 04:33:09', 1500.00, 'Efectivo', 1, NULL),
(8, 8, 7, '2026-10-06 16:08:09', 3200.00, 'Tarjeta de crédito', 1, NULL),
(9, 9, 7, '2026-10-07 03:08:09', 13500.00, 'Efectivo', 1, NULL),
(10, 10, 6, '2026-10-09 03:28:09', 51000.00, 'Transferencia', 1, NULL),
(11, 11, 7, '2026-10-07 10:18:09', 4500.00, 'Tarjeta de débito', 1, NULL),
(12, 12, 7, '2026-10-08 07:28:09', 22500.00, 'Efectivo', 1, NULL),
(13, 13, 7, '2026-10-08 00:23:09', 1600.00, 'Efectivo', 1, NULL),
(14, 14, 7, '2026-10-08 10:08:09', 7500.00, 'Tarjeta de crédito', 1, NULL),
(15, 15, 6, '2026-10-08 21:08:09', 15400.00, 'Efectivo', 1, NULL),
(16, 16, 7, '2026-10-09 23:28:09', 24000.00, 'Efectivo', 1, NULL),
(17, 17, 7, '2026-10-09 08:38:09', 3000.00, 'Efectivo', 1, NULL),
(18, 18, 7, '2026-10-09 18:28:09', 3200.00, 'Efectivo', 1, NULL),
(19, 19, 7, '2026-10-08 02:28:09', 0.00, 'Efectivo', 1, NULL),
(20, 20, 7, '2026-10-10 02:08:09', 0.00, 'Efectivo', 1, NULL),
(21, 21, 6, '2026-10-10 04:08:09', 2400.00, 'Tarjeta de débito', 1, NULL),
(22, 28, 6, '2026-10-10 17:58:41', 800.00, 'Efectivo', 1, NULL),
(23, 24, 6, '2026-10-10 21:59:11', 10500.00, 'Transferencia', 1, '/uploads/comprobantes/comprobante_23.pdf'),
(24, 29, 6, '2026-10-30 19:48:06', 240000.00, 'Transferencia', 1, '/uploads/comprobantes/comprobante_24.pdf');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tarifa`
--

CREATE TABLE `tarifa` (
  `id` int(11) NOT NULL,
  `descripcion` varchar(100) NOT NULL,
  `precio` decimal(10,2) NOT NULL,
  `unidad_tiempo` varchar(30) NOT NULL,
  `activa` tinyint(1) NOT NULL DEFAULT 1,
  `id_tipo_vehiculo` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `tarifa`
--

INSERT INTO `tarifa` (`id`, `descripcion`, `precio`, `unidad_tiempo`, `activa`, `id_tipo_vehiculo`) VALUES
(1, 'Auto - por hora', 1500.00, 'Hora', 1, 1),
(2, 'Auto - por día', 12000.00, 'Dia', 1, 1),
(3, 'Moto - por hora', 800.00, 'Hora', 1, 2),
(4, 'Moto - por día', 6000.00, 'Dia', 1, 2),
(5, 'Camioneta - por hora', 2200.00, 'Hora', 1, 3),
(6, 'Camioneta - por día', 17000.00, 'Dia', 1, 3),
(7, 'Auto - por hora (tarifa anterior)', 1200.00, 'Hora', 0, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tipo_vehiculo`
--

CREATE TABLE `tipo_vehiculo` (
  `id` int(11) NOT NULL,
  `descripcion` varchar(50) NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `tipo_vehiculo`
--

INSERT INTO `tipo_vehiculo` (`id`, `descripcion`, `activo`) VALUES
(1, 'Auto', 1),
(2, 'Moto', 1),
(3, 'Camioneta', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuario`
--

CREATE TABLE `usuario` (
  `id` int(11) NOT NULL,
  `nombre_usuario` varchar(50) NOT NULL,
  `password` varchar(255) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `apellido` varchar(50) NOT NULL,
  `rol` varchar(30) NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT 1,
  `avatar` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `usuario`
--

INSERT INTO `usuario` (`id`, `nombre_usuario`, `password`, `nombre`, `apellido`, `rol`, `activo`, `avatar`) VALUES
(6, 'admin', 'pLrZmCJByD7krzvsr+3fNqdNhk1m4j6SsLYnPECTYfo=', 'Marcelo', 'Lopez', 'Administrador', 1, '/uploads/avatares/avatar_6.png'),
(7, 'empleado', '56S1SoY6faofnMBRtA8zWc+pmjSx4bYB2kKk0tSK3do=', 'Adrian', 'Lucero', 'Empleado', 1, '/uploads/avatares/avatar_7.png');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `vehiculo`
--

CREATE TABLE `vehiculo` (
  `id` int(11) NOT NULL,
  `patente` varchar(20) NOT NULL,
  `marca` varchar(50) NOT NULL,
  `modelo` varchar(50) NOT NULL,
  `color` varchar(30) DEFAULT NULL,
  `id_cliente` int(11) DEFAULT NULL,
  `id_tipo_vehiculo` int(11) NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `vehiculo`
--

INSERT INTO `vehiculo` (`id`, `patente`, `marca`, `modelo`, `color`, `id_cliente`, `id_tipo_vehiculo`, `activo`) VALUES
(1, 'LE504YB', 'Volkswagen', 'Gol', 'Blanco', 1, 1, 1),
(2, 'UD474VB', 'Chevrolet', 'Onix', 'Azul', 2, 1, 1),
(3, 'GB188PP', 'Volkswagen', 'Gol', 'Blanco', 3, 1, 1),
(4, 'HC664PB', 'Ford', 'Fiesta', 'Gris', 4, 1, 1),
(5, 'VD328YY', 'Honda', 'Wave 110', 'Rojo', 5, 2, 1),
(6, 'VV506BH', 'Volkswagen', 'Gol', 'Blanco', 6, 1, 1),
(7, 'UE396PE', 'Chevrolet', 'Onix', 'Azul', 7, 1, 1),
(8, 'DV415UZ', 'Yamaha', 'FZ 150', 'Negro', 8, 2, 1),
(9, 'DV684YG', 'Motomel', 'Blitz 110', 'Azul', 9, 2, 1),
(10, 'DU829CV', 'Volkswagen', 'Gol', 'Blanco', 10, 1, 1),
(11, 'XG608ZU', 'Chevrolet', 'S10', 'Rojo', 11, 3, 1),
(12, 'LR699RM', 'Toyota', 'Etios', 'Rojo', 12, 1, 1),
(13, 'HF815HC', 'Chevrolet', 'Onix', 'Azul', 13, 1, 1),
(14, 'KT606LR', 'Motomel', 'Blitz 110', 'Azul', 14, 2, 1),
(15, 'XC220TP', 'Fiat', 'Cronos', 'Gris', 15, 1, 1),
(16, 'LE600PB', 'Toyota', 'Hilux', 'Blanco', 16, 3, 1),
(17, 'UV908LL', 'Peugeot', '208', 'Plata', 17, 1, 1),
(18, 'MX608VR', 'Volkswagen', 'Gol', 'Blanco', 18, 1, 1),
(19, 'CJ585ZC', 'Honda', 'Wave 110', 'Rojo', 19, 2, 1),
(20, 'KY691ZR', 'Toyota', 'Etios', 'Rojo', 20, 1, 1),
(21, 'NZ455AR', 'Volkswagen', 'Amarok', 'Negro', 21, 3, 1),
(22, 'FX219SB', 'Fiat', 'Cronos', 'Gris', 22, 1, 1),
(23, 'KE856HN', 'Renault', 'Sandero', 'Negro', 23, 1, 1),
(24, 'SC270RN', 'Motomel', 'Blitz 110', 'Azul', 24, 2, 1),
(25, 'EP984UJ', 'Peugeot', '208', 'Plata', NULL, 1, 1),
(26, 'PM799NH', 'Yamaha', 'FZ 150', 'Negro', NULL, 2, 1),
(27, 'CF254HZ', 'Fiat', 'Cronos', 'Gris', NULL, 1, 1),
(28, 'AS951VF', 'Volkswagen', 'Amarok', 'Negro', NULL, 3, 1),
(29, 'AB332XZ', 'Fiat', '125', 'Rojo', NULL, 1, 0),
(30, 'JT232CB', 'Ford', 'Ka', 'Rojo', 22, 1, 0),
(31, 'TEST123', 'Ford', 'Ka', 'Rojo', NULL, 1, 0);

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `abono`
--
ALTER TABLE `abono`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_abono_cliente` (`id_cliente`),
  ADD KEY `fk_abono_vehiculo` (`id_vehiculo`);

--
-- Indices de la tabla `cliente`
--
ALTER TABLE `cliente`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `dni` (`dni`);

--
-- Indices de la tabla `cochera`
--
ALTER TABLE `cochera`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `numero` (`numero`),
  ADD KEY `fk_cochera_tipo` (`id_tipo_vehiculo`);

--
-- Indices de la tabla `estadia`
--
ALTER TABLE `estadia`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_estadia_vehiculo` (`id_vehiculo`),
  ADD KEY `fk_estadia_cochera` (`id_cochera`),
  ADD KEY `fk_estadia_tarifa` (`id_tarifa`),
  ADD KEY `fk_estadia_abono` (`id_abono`),
  ADD KEY `fk_estadia_usuario` (`id_usuario`);

--
-- Indices de la tabla `pago`
--
ALTER TABLE `pago`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_pago_estadia` (`id_estadia`),
  ADD KEY `fk_pago_usuario` (`id_usuario_registro`);

--
-- Indices de la tabla `tarifa`
--
ALTER TABLE `tarifa`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_tarifa_tipo` (`id_tipo_vehiculo`);

--
-- Indices de la tabla `tipo_vehiculo`
--
ALTER TABLE `tipo_vehiculo`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `usuario`
--
ALTER TABLE `usuario`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre_usuario` (`nombre_usuario`);

--
-- Indices de la tabla `vehiculo`
--
ALTER TABLE `vehiculo`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `patente` (`patente`),
  ADD KEY `fk_vehiculo_cliente` (`id_cliente`),
  ADD KEY `fk_vehiculo_tipo` (`id_tipo_vehiculo`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `abono`
--
ALTER TABLE `abono`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT de la tabla `cliente`
--
ALTER TABLE `cliente`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=26;

--
-- AUTO_INCREMENT de la tabla `cochera`
--
ALTER TABLE `cochera`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=27;

--
-- AUTO_INCREMENT de la tabla `estadia`
--
ALTER TABLE `estadia`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=33;

--
-- AUTO_INCREMENT de la tabla `pago`
--
ALTER TABLE `pago`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=25;

--
-- AUTO_INCREMENT de la tabla `tarifa`
--
ALTER TABLE `tarifa`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `tipo_vehiculo`
--
ALTER TABLE `tipo_vehiculo`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `usuario`
--
ALTER TABLE `usuario`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `vehiculo`
--
ALTER TABLE `vehiculo`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=32;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `abono`
--
ALTER TABLE `abono`
  ADD CONSTRAINT `fk_abono_cliente` FOREIGN KEY (`id_cliente`) REFERENCES `cliente` (`id`),
  ADD CONSTRAINT `fk_abono_vehiculo` FOREIGN KEY (`id_vehiculo`) REFERENCES `vehiculo` (`id`);

--
-- Filtros para la tabla `cochera`
--
ALTER TABLE `cochera`
  ADD CONSTRAINT `fk_cochera_tipo` FOREIGN KEY (`id_tipo_vehiculo`) REFERENCES `tipo_vehiculo` (`id`);

--
-- Filtros para la tabla `estadia`
--
ALTER TABLE `estadia`
  ADD CONSTRAINT `fk_estadia_abono` FOREIGN KEY (`id_abono`) REFERENCES `abono` (`id`),
  ADD CONSTRAINT `fk_estadia_cochera` FOREIGN KEY (`id_cochera`) REFERENCES `cochera` (`id`),
  ADD CONSTRAINT `fk_estadia_tarifa` FOREIGN KEY (`id_tarifa`) REFERENCES `tarifa` (`id`),
  ADD CONSTRAINT `fk_estadia_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuario` (`id`),
  ADD CONSTRAINT `fk_estadia_vehiculo` FOREIGN KEY (`id_vehiculo`) REFERENCES `vehiculo` (`id`);

--
-- Filtros para la tabla `pago`
--
ALTER TABLE `pago`
  ADD CONSTRAINT `fk_pago_estadia` FOREIGN KEY (`id_estadia`) REFERENCES `estadia` (`id`),
  ADD CONSTRAINT `fk_pago_usuario` FOREIGN KEY (`id_usuario_registro`) REFERENCES `usuario` (`id`);

--
-- Filtros para la tabla `tarifa`
--
ALTER TABLE `tarifa`
  ADD CONSTRAINT `fk_tarifa_tipo` FOREIGN KEY (`id_tipo_vehiculo`) REFERENCES `tipo_vehiculo` (`id`);

--
-- Filtros para la tabla `vehiculo`
--
ALTER TABLE `vehiculo`
  ADD CONSTRAINT `fk_vehiculo_cliente` FOREIGN KEY (`id_cliente`) REFERENCES `cliente` (`id`) ON DELETE SET NULL,
  ADD CONSTRAINT `fk_vehiculo_tipo` FOREIGN KEY (`id_tipo_vehiculo`) REFERENCES `tipo_vehiculo` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
