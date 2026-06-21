package com.ngoconnect.entities;

import java.math.BigDecimal;
import java.sql.Date;
import java.time.LocalDate;

import com.fasterxml.jackson.annotation.JsonFormat;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "beneficiary_request")
@Getter
@Setter
public class BeneficiaryRequest {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int request_id;

    @Column(name = "item_id", nullable = false)
    private Integer itemId;

    @ManyToOne
    @JoinColumn(name = "item_id")
    private Item item;

    private String proofDocument;
    private BigDecimal amountNeeded;
    private String description;

    @Column(name = "request_date")
    @JsonFormat(pattern = "yyyy-MM-dd")
    private LocalDate requestDate;
    
    @Column(name = "expire_date", nullable = false)
    private LocalDate expiryDate;

}
