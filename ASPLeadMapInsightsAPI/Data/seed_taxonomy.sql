-- Таксономични данни за LeafMap Insights (идемпотентно – добавя само ако няма)
-- Изпълнение: вижте README_TAXONOMIC_IMPORT.md или bash командата по-долу.

SET NOCOUNT ON;

-- Divisions
INSERT INTO Divisions (Name) SELECT N'Magnoliophyta' WHERE NOT EXISTS (SELECT 1 FROM Divisions WHERE Name = N'Magnoliophyta');
INSERT INTO Divisions (Name) SELECT N'Pinophyta' WHERE NOT EXISTS (SELECT 1 FROM Divisions WHERE Name = N'Pinophyta');

-- TaxonomyClasses
INSERT INTO TaxonomyClasses (Name) SELECT N'Magnoliopsida' WHERE NOT EXISTS (SELECT 1 FROM TaxonomyClasses WHERE Name = N'Magnoliopsida');
INSERT INTO TaxonomyClasses (Name) SELECT N'Pinopsida' WHERE NOT EXISTS (SELECT 1 FROM TaxonomyClasses WHERE Name = N'Pinopsida');

-- Families
INSERT INTO Families (Name) SELECT N'Araliaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Araliaceae');
INSERT INTO Families (Name) SELECT N'Betulaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Betulaceae');
INSERT INTO Families (Name) SELECT N'Cornaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Cornaceae');
INSERT INTO Families (Name) SELECT N'Cupressaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Cupressaceae');
INSERT INTO Families (Name) SELECT N'Ericaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Ericaceae');
INSERT INTO Families (Name) SELECT N'Fabaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Fabaceae');
INSERT INTO Families (Name) SELECT N'Geraniaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Geraniaceae');
INSERT INTO Families (Name) SELECT N'Hydrangeaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Hydrangeaceae');
INSERT INTO Families (Name) SELECT N'Malvaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Malvaceae');
INSERT INTO Families (Name) SELECT N'Oleaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Oleaceae');
INSERT INTO Families (Name) SELECT N'Pinaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Pinaceae');
INSERT INTO Families (Name) SELECT N'Platanaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Platanaceae');
INSERT INTO Families (Name) SELECT N'Rosaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Rosaceae');
INSERT INTO Families (Name) SELECT N'Sapindaceae' WHERE NOT EXISTS (SELECT 1 FROM Families WHERE Name = N'Sapindaceae');

-- Genera
INSERT INTO Genera (Name) SELECT N'Abies' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Abies');
INSERT INTO Genera (Name) SELECT N'Acer' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Acer');
INSERT INTO Genera (Name) SELECT N'Aesculus' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Aesculus');
INSERT INTO Genera (Name) SELECT N'Betula' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Betula');
INSERT INTO Genera (Name) SELECT N'Cornus' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Cornus');
INSERT INTO Genera (Name) SELECT N'Cupressus' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Cupressus');
INSERT INTO Genera (Name) SELECT N'Fraxinus' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Fraxinus');
INSERT INTO Genera (Name) SELECT N'Geranium' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Geranium');
INSERT INTO Genera (Name) SELECT N'Hedera' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Hedera');
INSERT INTO Genera (Name) SELECT N'Hydrangea' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Hydrangea');
INSERT INTO Genera (Name) SELECT N'Juniperus' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Juniperus');
INSERT INTO Genera (Name) SELECT N'Picea' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Picea');
INSERT INTO Genera (Name) SELECT N'Pinus' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Pinus');
INSERT INTO Genera (Name) SELECT N'Platanus' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Platanus');
INSERT INTO Genera (Name) SELECT N'Platycladus' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Platycladus');
INSERT INTO Genera (Name) SELECT N'Robinia' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Robinia');
INSERT INTO Genera (Name) SELECT N'Rosa' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Rosa');
INSERT INTO Genera (Name) SELECT N'Sequoia' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Sequoia');
INSERT INTO Genera (Name) SELECT N'Spiraea' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Spiraea');
INSERT INTO Genera (Name) SELECT N'Tilia' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Tilia');
INSERT INTO Genera (Name) SELECT N'Vaccinium' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Vaccinium');
INSERT INTO Genera (Name) SELECT N'Ceratonia' WHERE NOT EXISTS (SELECT 1 FROM Genera WHERE Name = N'Ceratonia');

-- Species
INSERT INTO Species (Name) SELECT N'Abies alba' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Abies alba');
INSERT INTO Species (Name) SELECT N'Acer platanoides' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Acer platanoides');
INSERT INTO Species (Name) SELECT N'Acer pseudoplatanus' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Acer pseudoplatanus');
INSERT INTO Species (Name) SELECT N'Aesculus hippocastanum' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Aesculus hippocastanum');
INSERT INTO Species (Name) SELECT N'Betula pendula' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Betula pendula');
INSERT INTO Species (Name) SELECT N'Cornus mas' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Cornus mas');
INSERT INTO Species (Name) SELECT N'Cupressus sempervirens' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Cupressus sempervirens');
INSERT INTO Species (Name) SELECT N'Fraxinus excelsior' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Fraxinus excelsior');
INSERT INTO Species (Name) SELECT N'Geranium sanguineum' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Geranium sanguineum');
INSERT INTO Species (Name) SELECT N'Hedera helix' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Hedera helix');
INSERT INTO Species (Name) SELECT N'Hydrangea macrophylla' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Hydrangea macrophylla');
INSERT INTO Species (Name) SELECT N'Juniperus communis' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Juniperus communis');
INSERT INTO Species (Name) SELECT N'Picea abies' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Picea abies');
INSERT INTO Species (Name) SELECT N'Pinus sylvestris' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Pinus sylvestris');
INSERT INTO Species (Name) SELECT N'Platanus orientalis' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Platanus orientalis');
INSERT INTO Species (Name) SELECT N'Platycladus orientalis' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Platycladus orientalis');
INSERT INTO Species (Name) SELECT N'Robinia pseudoacacia' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Robinia pseudoacacia');
INSERT INTO Species (Name) SELECT N'Rosa canina' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Rosa canina');
INSERT INTO Species (Name) SELECT N'Sequoia sempervirens' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Sequoia sempervirens');
INSERT INTO Species (Name) SELECT N'Spiraea salicifolia' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Spiraea salicifolia');
INSERT INTO Species (Name) SELECT N'Tilia platyphyllos' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Tilia platyphyllos');
INSERT INTO Species (Name) SELECT N'Vaccinium myrtillus' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Vaccinium myrtillus');
INSERT INTO Species (Name) SELECT N'Ceratonia siliqua' WHERE NOT EXISTS (SELECT 1 FROM Species WHERE Name = N'Ceratonia siliqua');

PRINT 'Taxonomy seed completed.';
