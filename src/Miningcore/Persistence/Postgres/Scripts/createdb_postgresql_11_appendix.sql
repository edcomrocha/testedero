SET ROLE miningcore;

DROP TABLE shares;

CREATE TABLE shares
(
    poolid            TEXT                     NOT NULL,
    blockheight       BIGINT                   NOT NULL,
    difficulty        DOUBLE PRECISION         NOT NULL,
    networkdifficulty DOUBLE PRECISION         NOT NULL,
    miner             TEXT                     NOT NULL,
    worker            TEXT NULL,
    useragent         TEXT NULL,
    ipaddress         TEXT                     NOT NULL,
    source            TEXT NULL,
    created           TIMESTAMP WITH TIME ZONE NOT NULL
) PARTITION BY LIST (poolid);

CREATE INDEX IDX_SHARES_CREATED ON SHARES (created);
CREATE INDEX IDX_SHARES_MINER_DIFFICULTY on SHARES (miner, difficulty);
