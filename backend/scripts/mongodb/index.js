const convert = require('xml-js');
const fs = require('fs');

var xml = fs.readFileSync('../../src/Infrastructure/_Categories-xml-data/categories.xml', {encoding: 'UTF-8'})

var result = convert.xml2js(xml, {compact: false, spaces: 4});

var jsonCategories = [];
for (const category of result.elements[0].elements) {
    var jsonCat = {
        id: Number(category.attributes.id), 
        name: category.attributes.name
    };
    jsonCat['sub-categories'] = [];
    for(const subCat of category.elements){
        var jsonSubCat = {
            id: Number(subCat.attributes.id), 
            name: subCat.attributes.name
        };
        jsonSubCat['sub-sub-categories'] = [];
        for(const sub2Cat of subCat.elements){
            var jsonSub2Cat = {
                id: Number(sub2Cat.attributes.id), 
                name: sub2Cat.attributes.name
            };  
            jsonSubCat['sub-sub-categories'].push(jsonSub2Cat);
        }
        jsonCat['sub-categories'].push(jsonSubCat);
    }
    jsonCategories.push(jsonCat);
}

fs.writeFileSync('./categories.js', 'var categories = ' + JSON.stringify(jsonCategories));