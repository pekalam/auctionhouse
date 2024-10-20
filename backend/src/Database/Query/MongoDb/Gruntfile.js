const Handlebars = require("handlebars");
const fs = require("fs");

Handlebars.registerHelper("inc", function (value, options) {
    return parseInt(value) + 1;
});

String.prototype.removeHandlebarsExt = function () {
    return this.replace(".hbs", "")
}

module.exports = function (grunt) {

    // Project configuration.
    grunt.initConfig({
        cluster: {
            all: {
                mongos: ["mongodb"],
                config_servers: ["db-config1"],
                nodes: ["db-node1", "db-node2", "db-node3"]
            }
        },
        copy: {
            cluster: {
                files: [
                    { expand: true, cwd: 'cluster/', src: ['**', '!templates/**', '!shardtemplate/**', '!mongos/templates/**', '!**/*.hbs'], dest: '../../../../../compose/infrastructure/mongodb' },
                    // in order to run compose command in different root folders, these files need to be copied to many destination dirs
                    { cwd: '.', src: './mongocluster-dev-keyfile.txt', dest: '../../../../../compose/infrastructure/mongodb/mongocluster-keyfile.txt' },
                    { cwd: '.', src: './mongocluster-dev-keyfile.txt', dest: '../../../../../compose/infrastructure/mongocluster-keyfile.txt' },
                    { cwd: '.', src: './mongocluster-dev-keyfile.txt', dest: '../../../../../compose/mongocluster-keyfile.txt' },
                    { cwd: '.', src: './mongouser-dev-password.txt', dest: '../../../../../compose/infrastructure/mongodb/mongouser-password.txt' },
                    { cwd: '.', src: './mongouser-dev-password.txt', dest: '../../../../../compose/infrastructure/mongouser-password.txt' },
                    { cwd: '.', src: './mongouser-dev-password.txt', dest: '../../../../../compose/mongouser-password.txt' }
                ]
            }
        }
    });
    grunt.loadNpmTasks('grunt-contrib-copy');

    grunt.registerTask('cluster', 'Generate mongo cluster files', function () {
        var context = grunt.config.get("cluster.all");

        var templates = ["cluster/templates/docker-compose.yml.hbs", "cluster/templates/docker-compose.inmemory.yml.hbs", "cluster/templates/docker-compose.volumes.yml.hbs", "cluster/templates/docker-compose.prod.env.yml.hbs", "cluster/mongos/templates/scripts/init.js.hbs"];
        for (var tpath of templates) {
            var template = Handlebars.compile(grunt.file.read(tpath));
            var targetPath = tpath.replace("templates/", "").removeHandlebarsExt();
            grunt.log.writeln("Writing " + targetPath);
            grunt.file.write(targetPath, template(context));
        }

        var i = 1;
        for (const node of context.nodes) {
            var clusterShardFolder = "cluster/shards/shard_" + node
            grunt.file.mkdir(clusterShardFolder)
            for (const file of grunt.file.expand("cluster/shardtemplate/**")) {
                if(!grunt.file.isDir(file)){
                    continue
                }
                grunt.file.copy(file, file.replace("cluster/shardtemplate", clusterShardFolder))
            }

            var shardTemplates = ["cluster/templates/shardtemplate/scripts/config.js.hbs", "cluster/templates/shardtemplate/scripts/init_replicaset.js.hbs",
            "cluster/templates/shardtemplate/Dockerfile.hbs"]
            for (const tpath of shardTemplates) {
                var shardTemplate = Handlebars.compile(grunt.file.read(tpath));

                var shardContext = {
                    "shard_name": node,
                    "replSetName": "n"+i
                }
                var targetPath = tpath.replace("cluster/templates/shardtemplate", clusterShardFolder).removeHandlebarsExt();
                grunt.log.writeln("Writing " + targetPath);
                grunt.file.write(targetPath, shardTemplate(shardContext))
            }
            i++;
        }
    });

    grunt.registerTask('cluster-copy-to-compose', ['copy:cluster']);

    grunt.registerTask('clean', 'Clean generated files', function () {
        for (const file of grunt.file.expand("cluster/shards/shard_?*")) {
            grunt.log.writeln("deleting existing " + file)
            grunt.file.delete(file)
        }
        grunt.file.delete("cluster/shards")

    });


};