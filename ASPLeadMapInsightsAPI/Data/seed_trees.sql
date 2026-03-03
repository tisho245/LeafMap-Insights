-- Дървета с български имена („в зеленото“), общи координати и описание за Чинар.
-- Първо изпълнете seed_taxonomy.sql. Координати: 42.656683759116866, 24.745867485545126

SET NOCOUNT ON;

DECLARE @Lat FLOAT = 42.656683759116866;
DECLARE @Lng FLOAT = 24.745867485545126;
DECLARE @DescChinar NVARCHAR(MAX) = N'Чинар (Platan orientalis). Бисерът на източната растителност. Кората му изглежда като олющена, има сивозеленикав цвят и се отделя на неправилни плочки. Тя е красиво, мозаечно оцветена. Наесен /септември-октомври/ по клоните му висят на дълги дръжки топчести съплодия, съставени от дребни семена/орехчета/, снабдени с дълги власинки. Различаваме: източен, западен и яворолистен чинар. Различават се по нарязването на листата и по броя и големината на съплодията. При западния чинар – по две, но едри топчести съцветия, при източния – по 3-7, но по-дребни, при яворолистния – 1-3 съплодия. Като диворастящ вид у нас се среща само източния. Листата на източния чинар са длановидни, подобни на яворовите, с 5 до 7 дяла, но с остри редки зъбци и върхове. При западния са с 3 до 5 дяла, тъпи и плитко нарязани. При яворолистния – с 5 дяла, нарязани по-плитко от тези при източния. Листата имат дълга /8 см/ дръжка. Короната, гледана отстрани, е мощна, разперена, величествена. Дървото расте бързо и може да достигне до 50 м височина, а диаметър – до 4 м. Смята се, че пределната му възраст е 2000 години. Страда от ниските температури, затова ще го намерите само в долния планински пояс. Западният и яворолистният са по-устойчиви на студа. Чинарът е димоустойчив и газоустойчив вид.';

-- Ружа (Rosa canina)
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Ружа')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Ружа', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Rosaceae' AND g.Name = N'Rosa' AND s.Name = N'Rosa canina';

-- Секвоя
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Секвоя')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Секвоя', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Pinophyta' AND c.Name = N'Pinopsida' AND f.Name = N'Cupressaceae' AND g.Name = N'Sequoia' AND s.Name = N'Sequoia sempervirens';

-- Смрика
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Смрика')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Смрика', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Pinophyta' AND c.Name = N'Pinopsida' AND f.Name = N'Pinaceae' AND g.Name = N'Picea' AND s.Name = N'Picea abies';

-- Сребриста ела
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Сребриста ела')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Сребриста ела', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Pinophyta' AND c.Name = N'Pinopsida' AND f.Name = N'Pinaceae' AND g.Name = N'Abies' AND s.Name = N'Abies alba';

-- Спиреа
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Спиреа')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Спиреа', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Rosaceae' AND g.Name = N'Spiraea' AND s.Name = N'Spiraea salicifolia';

-- Хортензия
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Хортензия')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Хортензия', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Hydrangeaceae' AND g.Name = N'Hydrangea' AND s.Name = N'Hydrangea macrophylla';

-- Чинар (с пълно описание)
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Чинар')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Чинар', NULL, @DescChinar, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Platanaceae' AND g.Name = N'Platanus' AND s.Name = N'Platanus orientalis';

-- Черница
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Черница')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Черница', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Ericaceae' AND g.Name = N'Vaccinium' AND s.Name = N'Vaccinium myrtillus';

-- Акация
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Акация')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Акация', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Fabaceae' AND g.Name = N'Robinia' AND s.Name = N'Robinia pseudoacacia';

-- Бял бор
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Бял бор')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Бял бор', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Pinophyta' AND c.Name = N'Pinopsida' AND f.Name = N'Pinaceae' AND g.Name = N'Pinus' AND s.Name = N'Pinus sylvestris';

-- Бяла бреза
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Бяла бреза')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Бяла бреза', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Betulaceae' AND g.Name = N'Betula' AND s.Name = N'Betula pendula';

-- Ела
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Ела')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Ела', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Pinophyta' AND c.Name = N'Pinopsida' AND f.Name = N'Pinaceae' AND g.Name = N'Abies' AND s.Name = N'Abies alba';

-- Кипарис
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Кипарис')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Кипарис', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Pinophyta' AND c.Name = N'Pinopsida' AND f.Name = N'Cupressaceae' AND g.Name = N'Cupressus' AND s.Name = N'Cupressus sempervirens';

-- Клен
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Клен')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Клен', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Sapindaceae' AND g.Name = N'Acer' AND s.Name = N'Acer platanoides';

-- Конски кестен
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Конски кестен')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Конски кестен', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Sapindaceae' AND g.Name = N'Aesculus' AND s.Name = N'Aesculus hippocastanum';

-- Липа
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Липа')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Липа', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Malvaceae' AND g.Name = N'Tilia' AND s.Name = N'Tilia platyphyllos';

-- Планински ясен
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Планински ясен')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Планински ясен', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Oleaceae' AND g.Name = N'Fraxinus' AND s.Name = N'Fraxinus excelsior';

-- Рожков (Ceratonia siliqua)
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Рожков')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Рожков', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Fabaceae' AND g.Name = N'Ceratonia' AND s.Name = N'Ceratonia siliqua';

-- Смърч
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Смърч')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Смърч', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Pinophyta' AND c.Name = N'Pinopsida' AND f.Name = N'Cupressaceae' AND g.Name = N'Juniperus' AND s.Name = N'Juniperus communis';

-- Явор
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Явор')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Явор', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Sapindaceae' AND g.Name = N'Acer' AND s.Name = N'Acer pseudoplatanus';

-- Бръшлян
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Бръшлян')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Бръшлян', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Araliaceae' AND g.Name = N'Hedera' AND s.Name = N'Hedera helix';

-- Дрян
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Дрян')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Дрян', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Cornaceae' AND g.Name = N'Cornus' AND s.Name = N'Cornus mas';

-- Здравец
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Здравец')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Здравец', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Magnoliophyta' AND c.Name = N'Magnoliopsida' AND f.Name = N'Geraniaceae' AND g.Name = N'Geranium' AND s.Name = N'Geranium sanguineum';

-- Източна туя
IF NOT EXISTS (SELECT 1 FROM Trees WHERE Name = N'Източна туя')
INSERT INTO Trees (Name, PhotoURL, Description, Latitude, Longitude, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId)
SELECT N'Източна туя', NULL, NULL, @Lat, @Lng, d.Id, c.Id, g.Id, f.Id, s.Id
FROM Divisions d, TaxonomyClasses c, Genera g, Families f, Species s
WHERE d.Name = N'Pinophyta' AND c.Name = N'Pinopsida' AND f.Name = N'Cupressaceae' AND g.Name = N'Platycladus' AND s.Name = N'Platycladus orientalis';

PRINT 'Trees seed completed.';
