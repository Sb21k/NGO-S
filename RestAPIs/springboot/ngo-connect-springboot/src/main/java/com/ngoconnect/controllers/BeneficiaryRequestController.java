package com.ngoconnect.controllers;

import java.io.IOException;
import java.math.BigDecimal;
import java.nio.file.Paths;
import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import com.ngoconnect.entities.BeneficiaryRequest;
import com.ngoconnect.services.BeneficiaryRequestService;

import java.nio.file.Path;
import org.springframework.core.io.Resource;
import org.springframework.core.io.UrlResource;


@RestController
@RequestMapping("/beneficiary")
//@CrossOrigin(origins = "http://localhost:5173")
public class BeneficiaryRequestController {

    @Autowired
    private BeneficiaryRequestService service;

    // CREATE REQUEST WITH PROOF
    @PostMapping(
        value = "/request",
        consumes = MediaType.MULTIPART_FORM_DATA_VALUE
    )
    public BeneficiaryRequest create(
            @RequestParam int beneficiaryId,
            @RequestParam int itemId,
            @RequestParam(required = false) BigDecimal amountNeeded,
            @RequestParam String description,
            @RequestParam String expiryDate, 
            @RequestParam MultipartFile proof
    ) {
    	
    	
        return service.createRequest(
            beneficiaryId,
            itemId,
            amountNeeded,
            description,
            expiryDate,
            proof
        );
    }

    // 2️⃣ Beneficiary views own requests
    @GetMapping("/requests/{beneficiaryId}")
    public List<BeneficiaryRequest> getRequestsByBeneficiary(
            @PathVariable Integer beneficiaryId) {
        return service.getRequestsByBeneficiary(beneficiaryId);
    }

    //update amount, description, proof
    @PutMapping(
    	    value = "/request/{id}",
    	    consumes = MediaType.MULTIPART_FORM_DATA_VALUE
    	)
    	public BeneficiaryRequest updateRequest(
    	        @PathVariable int id,
    	        @RequestParam(required = false) BigDecimal amountNeeded,
    	        @RequestParam String description,
    	        @RequestParam(required = false) MultipartFile proof
    	) {
    	    return service.updateRequest(id, amountNeeded, description, proof);
    	}




   // DELETE REQUEST
    @DeleteMapping("/request/{id}")
    public void delete(@PathVariable int id) {
        service.deleteRequest(id);
    }
    
    
    @GetMapping("/proof/{filename}")
    public ResponseEntity<Resource> getProof(@PathVariable String filename) throws IOException {

        Path path = Paths.get("uploads").resolve(filename).normalize();

        if (!path.toFile().exists()) {
            return ResponseEntity.notFound().build();
        }

        Resource resource = new UrlResource(path.toUri());

        return ResponseEntity.ok()
                .contentType(MediaType.APPLICATION_OCTET_STREAM)
                .body(resource);
    }


}
