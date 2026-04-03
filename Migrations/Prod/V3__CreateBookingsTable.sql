CREATE TABLE prod.Bookings (
    BookingId BIGSERIAL NOT NULL,
    UserId BIGINT NOT NULL,
    RoomId BIGINT NOT NULL,
    StartTime TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    EndTime TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT pk_bookings PRIMARY KEY (booking_id),
    CONSTRAINT fk_bookings_user_id FOREIGN KEY (UserId) 
        REFERENCES prod.Users(UserId) 
        ON DELETE CASCADE
        ON UPDATE NO ACTION,
    CONSTRAINT fk_bookings_room_id FOREIGN KEY (RoomId) 
        REFERENCES prod.Rooms(RoomId) 
        ON DELETE CASCADE
        ON UPDATE NO ACTION
);