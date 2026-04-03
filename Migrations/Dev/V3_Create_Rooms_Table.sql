CREATE TABLE dev.Rooms (
    RoomId BIGSERIAL NOT NULL,
    Description TEXT NOT NULL,
    Address TEXT NOT NULL,
    PricePerHour NUMERIC(10, 2) NOT NULL,
    OwnerId BIGINT NOT NULL,
    version BIGINT NOT NULL DEFAULT 0,
    CONSTRAINT pk_rooms PRIMARY KEY (RoomId),
    CONSTRAINT fk_rooms_owner_id FOREIGN KEY (OwnerId) 
        REFERENCES dev.Users(UserId) 
        ON DELETE CASCADE
        ON UPDATE NO ACTION
);