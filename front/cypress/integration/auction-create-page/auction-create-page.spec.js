/// <reference types="Cypress" />

const signalR = require("@aspnet/signalr");

class SelectCategoryPageObj {
  static checkVisibility(cat1Visible, cat2Visible, cat3Visible) {
    cy.get("#mat-select-0", { timeout: 10000 }).should(
      cat1Visible ? "be.visible" : "not.be.visible"
    );
    cy.get("#mat-select-1", { timeout: 10000 }).should(
      cat2Visible ? "be.visible" : "not.be.visible"
    );
    cy.get("#mat-select-2", { timeout: 10000 }).should(
      cat3Visible ? "be.visible" : "not.be.visible"
    );
  }

  static selectCat1() {
    cy.get("#mat-select-0", { timeout: 10000 }).click();
    cy.get("#main-cat-0", { timeout: 10000 }).click();
  }

  static selectCat2() {
    cy.get("#mat-select-1", { timeout: 10000 }).click();
    cy.get("#sub-cat-0", { timeout: 10000 }).click();
  }

  static selectCat3() {
    cy.get("#mat-select-2", { timeout: 10000 }).click();
    cy.get("#sub2-cat-0", { timeout: 10000 }).click();
  }

  static fillAllFields() {
    this.checkVisibility(true, false, false);
    cy.get(".create-btn").should("not.be.visible");
    this.selectCat1();
    this.checkVisibility(true, true, false);
    cy.get(".create-btn").should("not.be.visible");
    this.selectCat2();
    this.checkVisibility(true, true, true);
    cy.get(".create-btn").should("not.be.visible");
    this.selectCat3();
    this.checkVisibility(true, true, true);
    cy.get(".create-btn").should("be.visible");
  }
}

class AuctionDataStepPagObj {
  static typeAuctionName(name) {
    cy.get("input[formcontrolname=name]").type(name);
  }

  static buyNow(isBuyNow) {
    isBuyNow
      ? cy.get("#buynow-checkbox-input").check({ force: true })
      : cy.get("#buynow-checkbox-input").uncheck({ force: true });
    isBuyNow
      ? cy.get("input[formcontrolname=buyNowPrice]").should("be.enabled")
      : cy.get("input[formcontrolname=buyNowPrice]").should("not.be.enabled");
  }

  static buyNowPrice(price) {
    cy.get("input[formcontrolname=buyNowPrice]").type(price);
  }

  static fillAllFields() {
    this.typeAuctionName("Test auction name");
    this.buyNow(true);
    this.buyNowPrice(20);
  }
}

class ProductStepPageObj {
  static typeProductName(name) {
    cy.get("input[formcontrolname=productName]").type(name);
  }

  static typeProductDescription(description) {
    cy.get("textarea[formcontrolname=productDescription]").type(description);
  }

  static typeTags(tagsStr) {
    cy.get("input[formcontrolname=tags]").type(tagsStr);
    cy.get(".tags-container")
      .children()
      .should("have.length", tagsStr.split(" ").length);
  }

  static fillAllFields() {
    this.typeProductName("e2e test product name");
    this.typeProductDescription("Product description");
    this.typeTags("tag1 tag2 tag3");
  }
}

class AddImagesPageObj {
  constructor() {
    this.added = 0;
  }

  addTestImgs(count) {
    for (let i = 0; i < count; i++) {
      this.addImg("test_image.jpg", i, "image/jpeg");
    }
  }

  addImg(imgName, num, mimeTypeStr) {
    cy.fixture(`auction-create-page/${imgName}`).then(fContent => {
      this.added++;

      cy.get(`#upload-photo-${num}`).upload({
        fileName: imgName,
        fileContent: fContent,
        mimeType: mimeTypeStr,
        encoding: "base64"
      });
    });
    cy.get(`#preview-container-${num}`, { timeout: 20000 }).should(
      "be.visible"
    );
    cy.get(`#preview-container-${num} > .cancel-btn`).should("be.visible");
    this.checkAreImgBtnsVisible(this.added > 3 ? this.added : 3);
  }

  removeImg(imgNum) {
    cy.get(`#preview-container-${imgNum} > .cancel-btn`)
      .should("be.visible")
      .click()
      .should("not.exist");
    this.added--;
    this.checkAreImgBtnsVisible(this.added > 3 ? this.added : 3);
  }

  checkAreImgBtnsVisible(count) {
    for (let i = 0; i < count; i++) {
      cy.get(`#add-img-btn-${i}`).should("be.visible");
    }
  }
}

function listenForRequestCompletion(commandId) {
  return new Promise((resolve, reject) => {
    
    var maxStatusFetch = 60;
    var interval = setInterval(async () => {
      const response = await fetch('https://localhost:7263/api/s/status/' + commandId)
      const json = await response.json()
      console.log(json)
      if(json['status'] == 'COMPLETED'){
        clearInterval(interval)
        resolve();
      }
      if(json['status'] == 'FAILED'){
        clearInterval(interval)
        resolve(); 
      }

      maxStatusFetch--;
      if(maxStatusFetch < 0){
        clearInterval(interval)
        reject();
      }
    }, 1000)

/*     let jwt = localStorage.getItem("user");
    console.log(jwt);

    let connection = new signalR.HubConnectionBuilder()
      .withUrl(`http://localhost:5000/app?token=${jwt}`)
      .build();

    connection.onclose(err => {
      console.log("connection closed by server ");
      console.log(err);
      reject();
    });

    connection.on("completed", responseStatus =>
      resolve({
        correlationId: responseStatus.correlationId,
        status: responseStatus.status
      })
    );

    connection.on("failed", responseStatus =>
      resolve({
        correlationId: responseStatus.correlationId,
        status: responseStatus.status
      })
    );

    connection
      .start()
      .then(() => {
        console.log("connection initialized");
      })
      .catch(err => {
        console.log(err);
        throw err;
      }); */
  });
}

describe("Auction create page steps", function() {
  this.beforeEach(() => {
    cy.server();
    cy.route({
      method: "POST",
      url: "/api/c/signin"
    }).as("api");

    cy.visit("http://localhost:4200/sign-in");

    cy.get("input[formcontrolname=username]").type("test1");
    cy.get("input[formcontrolname=password]").type("pass");
    cy.get("#sign-in-button")
      .click()
      .then(() => {
        cy.wait("@api").then(xhr => {
          assert.equal(xhr.status, 200, "User test should be able to sign in");
        });
      });
  });

  it("Shows page after successful login", function() {
    cy.server();
    cy.route({
      method: "POST",
      url: "/api/c/createAuction"
    }).as("api");

    cy.url().should("eq", "http://localhost:4200/home");
    cy.get("#sell-btn").click();
    cy.url().should("eq", "http://localhost:4200/auctions/create");

    SelectCategoryPageObj.fillAllFields();
    cy.get(".create-btn")
      .should("be.visible")
      .click();
    cy.get(".back").should("be.visible");
    AuctionDataStepPagObj.fillAllFields();
    cy.get(".create-btn")
      .should("be.visible")
      .click();
    cy.get(".back").should("be.visible");
    ProductStepPageObj.fillAllFields();
    cy.get(".create-btn")
      .should("be.visible")
      .click();

    const addImagesPageObj = new AddImagesPageObj();
    addImagesPageObj.addTestImgs(4);
    cy.get(".create-btn")
      .should("be.visible")
      .click();

    cy.get(".create-btn")
      .should($el => {
        const txt = $el.text().toString();
        expect(txt).to.have.string("Create auction");
      })
      .click()
      .then(_ => {
        cy.wait("@api")
          .then({timeout: 15000}, xhr => {
            console.log(xhr.responseBody)

            assert.equal(
              xhr.status,
              200,
              "Create auction request should respond with 200 status code"
            );
            timeout = setTimeout(() => {
              console.log(
                "Create auction server message was not received within given time"
              );
              assert.fail();
            }, 7000);
            return listenForRequestCompletion(xhr.responseBody.commandId);
          })
          .then({timeout: 15000}, result => {
            console.log("RESOLVED: " + result.correlationId);
            clearTimeout(timeout);
          });
      });

    var timeout;
  });
});
